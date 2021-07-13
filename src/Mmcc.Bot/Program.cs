using System;
using System.IO;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Caching;
using Mmcc.Bot.CommandGroups.Core;
using Mmcc.Bot.CommandGroups.Diagnostics;
using Mmcc.Bot.CommandGroups.Minecraft;
using Mmcc.Bot.CommandGroups.Moderation;
using Mmcc.Bot.CommandGroups.Tags;
using Mmcc.Bot.Core;
using Mmcc.Bot.Core.Extensions.Database;
using Mmcc.Bot.Core.Extensions.Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Models.Settings;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Settings;
using Mmcc.Bot.Infrastructure.Behaviours;
using Mmcc.Bot.Infrastructure.Commands.MemberApplications;
using Mmcc.Bot.Infrastructure.Conditions;
using Mmcc.Bot.Infrastructure.HostedServices;
using Mmcc.Bot.Infrastructure.Parsers;
using Mmcc.Bot.Infrastructure.Queries.Core;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.Responders.Guilds;
using Mmcc.Bot.Responders.Messages;
using Mmcc.Bot.Responders.Users;
using Mmcc.Bot.Protos;
using Mmcc.Bot.RemoraAbstractions;
using Mmcc.Bot.Responders.Buttons;
using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Services;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;
using Ssmp.Extensions;

namespace Mmcc.Bot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var isDevelopment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")?.Equals("Development") ??
                                false;

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System.Net.Http.HttpClient",
                    isDevelopment ? LogEventLevel.Information : LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:dd/MM/yyyy HH:mm:ss:fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    theme: isDevelopment ? null : AnsiConsoleTheme.Literate
                )
                .WriteTo.File(
                    new CompactJsonFormatter(),
                    Path.Combine("logs", "log.clef"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    levelSwitch: new LoggingLevelSwitch(LogEventLevel.Warning)
                )
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
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // add config;
                    services.AddConfigWithValidation<MySqlSettings, MySqlSettingsValidator>(
                        hostContext.Configuration.GetSection("MySql"));
                    services.AddConfigWithValidation<DiscordSettings, DiscordSettingsValidator>(
                        hostContext.Configuration.GetSection("Discord"));
                    services.AddConfigWithValidation<PolychatSettings, PolychatSettingsValidator>(
                        hostContext.Configuration.GetSection("Polychat"));
                    
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
                        var dbConfig = provider.GetRequiredService<MySqlSettings>();
                        var connString = dbConfig.ConnectionString;
                        var serverVersion = ServerVersion.Parse(dbConfig.MySqlVersionString);
                        var retryAmount = dbConfig.RetryAmount;

                        options.UseMySql(connString, serverVersion,
                            contextOptions => contextOptions.EnableRetryOnFailure(retryAmount));
                    });

                    services.AddSingleton<IColourPalette, TailwindColourPalette>();
                    services.AddSingleton<IButtonHandlerRepository, ButtonHandlerRepository>();
                    
                    services.AddScoped<IDiscordSanitiserService, DiscordSanitiserService>();
                    services.AddScoped<IHelpService, HelpService>();
                    services.AddScoped<IDmSender, DmSender>();
                    services.AddScoped<IDiscordPermissionsService, DiscordPermissionsService>();
                    services.AddScoped<IExecutionEventService, ErrorNotificationService>();
                    services.AddScoped<IMojangApiService, MojangApiService>();
                    services.AddScoped<IModerationService, ModerationService>();
                    services.AddScoped<ICommandResponder, CommandResponder>();
                    services.AddScoped<IInteractionResponder, InteractionResponder>();

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
                    services.AddCommandGroup<MmccInfoCommands>();
                    
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
                    services.AddDiscordCaching();
                    
                    // set up Ssmp central server;
                    var ssmpConfig = hostContext.Configuration.GetSection("Ssmp");
                    services.AddSsmp<SsmpHandler>(ssmpConfig);

                    // set up Polychat2;
                    services.AddSingleton<IPolychatService, PolychatService>();
                    services.AddHostedService<BroadcastsHostedService>();
                    
                    services.AddHostedService<DiscordService>();
                    services.AddHostedService<ModerationBackgroundService>();

                    services.AddResponder<ButtonInteractionCreateResponder>();
                })
                .UseDefaultServiceProvider(options => options.ValidateScopes = true)
                .UseSerilog();
    }
}