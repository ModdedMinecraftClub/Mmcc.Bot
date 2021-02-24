using System;
using System.Collections.Generic;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mmcc.Bot.CommandGroups;
using Mmcc.Bot.CommandGroups.Moderation;
using Mmcc.Bot.Core.Models.Settings;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Settings;
using Mmcc.Bot.Infrastructure;
using Mmcc.Bot.Infrastructure.Commands.MemberApplications;
using Mmcc.Bot.Infrastructure.Conditions;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.Infrastructure.Workers;
using Mmcc.Bot.Responders;
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
                            | GatewayIntents.GuildBans
                            | GatewayIntents.GuildMessages;
                    });
                    
                    services.AddDbContext<BotContext>((provider, options) =>
                    {
                        var config = provider.GetRequiredService<MySqlSettings>();
                        var connString =
                            $"Server={config.ServerIp};Port={config.Port};Database={config.DatabaseName};Uid={config.Username};Pwd={config.Password};Allow User Variables=True";
                        var serverVersion = ServerVersion.FromString("10.4.11-mariadb");
                        
                        options.UseMySql(connString, serverVersion);
                    });
                    
                    services.AddTailwindColourPalette();
                    services.AddScoped<IExecutionEventService, ErrorNotificationService>();
                    services.AddScoped<IPolychatCommunicationService, PolychatCommunicationService>();
                    services.AddScoped<IMojangApiService, MojangApiService>();
                    services.AddScoped<IModerationService, ModerationService>();

                    services.AddMediatR(typeof(CreateFromDiscordMessage));
                    
                    services.AddDiscordCommands();
                    
                    services.AddCondition<RequireGuildCondition>();
                    services.AddCondition<RequireUserGuildPermissionCondition>();

                    services.AddCommandGroup<TestCommands>();
                    services.AddCommandGroup<MemberApplicationsCommands>();
                    services.AddCommandGroup<HelpCommands>();
                    
                    // moderation;
                    services.AddCommandGroup<PlayerInfoCommands>();
                    services.AddCommandGroup<BanCommands>();
                    services.AddCommandGroup<WarnCommands>();

                    services.AddResponder<MemberApplicationCreatedResponder>();
                    services.AddResponder<MemberApplicationUpdatedResponder>();
                    
                    services.AddDiscordGateway(provider =>
                    {
                        var discordConfig = provider.GetRequiredService<DiscordSettings>();
                        return discordConfig.Token;
                    });

                    services.AddSingleton<DiscordService>();
                    services.AddSingleton<IHostedService, DiscordService>(serviceProvider =>
                        serviceProvider.GetRequiredService<DiscordService>());
                    services.AddSingleton<ModerationWorker>();
                    services.AddSingleton<IHostedService, ModerationWorker>(serviceProvider =>
                        serviceProvider.GetRequiredService<ModerationWorker>());

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

                        settings.SetAbsoluteExpiration<IGuildMember>(TimeSpan.FromHours(12));
                        settings.SetSlidingExpiration<IGuildMember>(TimeSpan.FromHours(3));
                    });
                });
    }
}