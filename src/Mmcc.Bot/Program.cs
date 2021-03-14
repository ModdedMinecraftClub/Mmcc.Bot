using System;
using System.Collections.Generic;
using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mmcc.Bot.CommandGroups;
using Mmcc.Bot.CommandGroups.Diagnostics;
using Mmcc.Bot.CommandGroups.Minecraft;
using Mmcc.Bot.CommandGroups.Moderation;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Models.Settings;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Settings;
using Mmcc.Bot.Infrastructure.Commands.MemberApplications;
using Mmcc.Bot.Infrastructure.Conditions;
using Mmcc.Bot.Infrastructure.HostedServices;
using Mmcc.Bot.Infrastructure.Parsers;
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
using Ssmp;

namespace Mmcc.Bot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
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
                        var serverVersion = ServerVersion.FromString("10.4.11-mariadb");

                        options.UseMySql(connString, serverVersion,
                            contextOptions => contextOptions.EnableRetryOnFailure(3));
                    });
                    
                    services.AddTailwindColourPalette();
                    
                    services.AddScoped<IExecutionEventService, ErrorNotificationService>();
                    services.AddScoped<IMojangApiService, MojangApiService>();
                    services.AddScoped<IModerationService, ModerationService>();
                    services.AddScoped<ITcpMessageProcessingService, TcpMessageProcessingService>();

                    services.AddMediatR(typeof(CreateFromDiscordMessage));
                    
                    services.AddDiscordCommands();
                    
                    services.AddCondition<RequireGuildCondition>();
                    services.AddCondition<RequireUserGuildPermissionCondition>();

                    services.AddParser<TimeSpan, TimeSpanParser>();
                    services.AddParser<ExpiryDate, ExpiryDateParser>();
                    
                    // core commands;
                    services.AddCommandGroup<HelpCommands>();

                    // diagnostics;
                    services.AddCommandGroup<DiagnosticsCommands>();
                    
                    // in game;
                    services.AddCommandGroup<MinecraftServersCommands>();
                    
                    // member apps;
                    services.AddCommandGroup<MemberApplicationsCommands>();

                    // moderation;
                    services.AddCommandGroup<PlayerInfoCommands>();
                    services.AddCommandGroup<BanCommands>();
                    services.AddCommandGroup<WarnCommands>();

                    services.AddResponder<GuildCreatedResponder>();
                    services.AddResponder<UserJoinedResponder>();
                    services.AddResponder<UserLeftResponder>();
                    services.AddResponder<FeedbackPostedResponder>();
                    services.AddResponder<MemberApplicationCreatedResponder>();
                    services.AddResponder<MemberApplicationUpdatedResponder>();

                    services.AddDiscordGateway(provider =>
                    {
                        var discordConfig = provider.GetRequiredService<DiscordSettings>();
                        return discordConfig.Token;
                    });
                    
                    // set up central server;
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
                                logger.LogError("Error in the central server's TCP byte[] message handler.", e);
                            }
                        },
                        1024, // TODO: make configurable;
                        IPAddress.Loopback,
                        provider.GetRequiredService<PolychatSettings>().Port
                        ));
                    services.AddSingleton<CentralServerBackgroundService>();
                    services.AddSingleton<IHostedService, CentralServerBackgroundService>(provider =>
                        provider.GetRequiredService<CentralServerBackgroundService>());

                    services.AddSingleton<DiscordService>();
                    services.AddSingleton<IHostedService, DiscordService>(provider =>
                        provider.GetRequiredService<DiscordService>());
                    services.AddSingleton<ModerationBackgroundService>();
                    services.AddSingleton<IHostedService, ModerationBackgroundService>(provider =>
                        provider.GetRequiredService<ModerationBackgroundService>());

                    services.AddDiscordCaching();
                    services.Configure<CacheSettings>(settings =>
                    {
                        settings.SetAbsoluteExpiration<IGuild>(TimeSpan.FromDays(2));
                        settings.SetSlidingExpiration<IGuild>(TimeSpan.FromDays(1));
                        
                        settings.SetAbsoluteExpiration<IRole>(TimeSpan.FromDays(2));
                        settings.SetSlidingExpiration<IRole>(TimeSpan.FromDays(1));
                        
                        settings.SetAbsoluteExpiration<IReadOnlyList<IRole>>(TimeSpan.FromDays(2));
                        settings.SetSlidingExpiration<IReadOnlyList<IRole>>(TimeSpan.FromDays(1));

                        settings.SetAbsoluteExpiration<IChannel>(TimeSpan.FromDays(1));
                        settings.SetSlidingExpiration<IChannel>(TimeSpan.FromHours(6));

                        settings.SetAbsoluteExpiration<IGuildMember>(TimeSpan.FromHours(6));
                        settings.SetSlidingExpiration<IGuildMember>(TimeSpan.FromHours(1));
                    });
                });
    }
}