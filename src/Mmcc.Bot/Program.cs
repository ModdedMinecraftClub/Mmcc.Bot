using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mmcc.Bot.CommandGroups;
using Mmcc.Bot.CommandGroups.Core;
using Mmcc.Bot.CommandGroups.Diagnostics;
using Mmcc.Bot.CommandGroups.Minecraft;
using Mmcc.Bot.CommandGroups.Moderation;
using Mmcc.Bot.CommandGroups.Tags;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Models.Settings;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Settings;
using Mmcc.Bot.Infrastructure.Behaviours;
using Mmcc.Bot.Infrastructure.Commands.MemberApplications;
using Mmcc.Bot.Infrastructure.Conditions;
using Mmcc.Bot.Infrastructure.HostedServices;
using Mmcc.Bot.Infrastructure.Parsers;
using Mmcc.Bot.Infrastructure.Queries;
using Mmcc.Bot.Infrastructure.Queries.Core;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.Responders.Guilds;
using Mmcc.Bot.Responders.Messages;
using Mmcc.Bot.Responders.Users;
using Mmcc.Bot.Setup;
using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Caching.Services;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Services;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Ssmp;

namespace Mmcc.Bot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("logs", "log.txt"), rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 3, levelSwitch: new LoggingLevelSwitch(LogEventLevel.Warning))
                .CreateLogger();

            try
            {
                Log.Information("Starting the host...");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, builder) =>
                {
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddDebug();
                    }

                    builder.AddSystemdConsole(options =>
                    {
                        options.TimestampFormat = "[dd/MM/yyyy HH:mm:ss] ";
                    });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // add config;
                    services.Configure<MySqlSettings>(hostContext.Configuration.GetSection("MySql"));
                    services.AddSingleton(provider => provider.GetRequiredService<IOptions<MySqlSettings>>().Value);
                    services.Configure<DiscordSettings>(hostContext.Configuration.GetSection("Discord"));
                    services.AddSingleton(provider => provider.GetRequiredService<IOptions<DiscordSettings>>().Value);
                    services.Configure<PolychatSettings>(hostContext.Configuration.GetSection("Polychat"));
                    services.AddSingleton(provider => provider.GetRequiredService<IOptions<PolychatSettings>>().Value);
                    
                    services.Configure<DiscordGatewayClientOptions>(options =>
                    {
                        options.Intents =
                            GatewayIntents.Guilds
                            | GatewayIntents.DirectMessages
                            | GatewayIntents.GuildMembers
                            | GatewayIntents.GuildBans
                            | GatewayIntents.GuildMessages
                            | GatewayIntents.GuildMessageReactions;
                    });
                    
                    services.AddDbContext<BotContext>((provider, options) =>
                    {
                        var config = provider.GetRequiredService<MySqlSettings>();
                        var connString =
                            $"Server={config.ServerIp};Port={config.Port};Database={config.DatabaseName};Uid={config.Username};Pwd={config.Password};Allow User Variables=True";
                        var serverVersion = ServerVersion.Parse("10.4.11-mariadb");

                        options.UseMySql(connString, serverVersion,
                            contextOptions => contextOptions.EnableRetryOnFailure(3));
                    });
                    
                    services.AddTailwindColourPalette();

                    services.AddSingleton<IDiscordSanitiserService, DiscordSanitiserService>();
                    
                    services.AddScoped<IExecutionEventService, ErrorNotificationService>();
                    services.AddScoped<IMojangApiService, MojangApiService>();
                    services.AddScoped<IModerationService, ModerationService>();
                    services.AddScoped<ITcpMessageProcessingService, TcpMessageProcessingService>();

                    services.AddValidatorsFromAssemblyContaining<GetGuildInfo>();
                    services.AddMediatR(typeof(CreateFromDiscordMessage));
                    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
                    
                    services.AddDiscordCommands();
                    
                    services.AddCondition<RequireGuildCondition>();
                    services.AddCondition<RequireUserGuildPermissionCondition>();

                    services.AddParser<TimeSpan, TimeSpanParser>();
                    services.AddParser<ExpiryDate, ExpiryDateParser>();
                    
                    // core commands;
                    services.AddCommandGroup<HelpCommands>();
                    services.AddCommandGroup<CoreGuildCommands>();
                    
                    // tags;
                    services.AddCommandGroup<TagsManagementCommands>();
                    services.AddCommandGroup<TagsUsageCommands>();

                    // diagnostics;
                    services.AddCommandGroup<DiagnosticsCommands>();
                    
                    // in game;
                    services.AddCommandGroup<MinecraftServersCommands>();
                    
                    // member apps;
                    services.AddCommandGroup<MemberApplicationsCommands>();

                    // moderation;
                    services.AddCommandGroup<GeneralModerationCommands>();
                    services.AddCommandGroup<PlayerInfoCommands>();
                    services.AddCommandGroup<BanCommands>();
                    services.AddCommandGroup<WarnCommands>();

                    services.AddResponder<GuildCreatedResponder>();
                    services.AddResponder<UserJoinedResponder>();
                    services.AddResponder<UserLeftResponder>();
                    services.AddResponder<FeedbackPostedResponder>();
                    services.AddResponder<FeedbackAddressedResponder>();
                    services.AddResponder<MemberApplicationCreatedResponder>();
                    services.AddResponder<MemberApplicationUpdatedResponder>();
                    services.AddResponder<DiscordChatMessageResponder>();

                    services.AddDiscordGateway(provider =>
                    {
                        var discordConfig = provider.GetRequiredService<DiscordSettings>();
                        return discordConfig.Token;
                    });
                    
                    // set up polychat central server;
                    services.AddSingleton<IPolychatService, PolychatService>();
                    services.AddSingleton(provider => new CentralServerService(
                        async (client, message) =>
                        {
                            using var scope = provider.CreateScope();
                            var handlingService = scope.ServiceProvider.GetRequiredService<ITcpMessageProcessingService>();
                            var logger = scope.ServiceProvider.GetRequiredService<ILogger<CentralServerService>>();
                            try
                            {
                                await handlingService.Handle(client, message);
                            }
                            catch (Exception e)
                            {
                                logger.LogError($"Error in the central server's TCP byte[] message handler.\n{e.StackTrace}");
                            }
                        },
                        provider.GetRequiredService<PolychatSettings>().MessageQueueLimit,
                        IPAddress.Loopback,
                        provider.GetRequiredService<PolychatSettings>().Port
                        ));
                    services.AddHostedService<CentralServerBackgroundService>();
                    services.AddHostedService<BroadcastsHostedService>();
                    
                    services.AddHostedService<DiscordService>();
                    services.AddHostedService<ModerationBackgroundService>();

                    services.AddDiscordCaching();
                })
                .UseSerilog();
    }
}