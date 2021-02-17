using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mmcc.Bot.CommandGroups;
using Mmcc.Bot.Core.Models.Settings;
using Mmcc.Bot.Infrastructure.Conditions;
using Mmcc.Bot.Infrastructure.HostedServices;
using Mmcc.Bot.Infrastructure.Services;
using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Caching.Services;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Extensions;
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

                    services.AddDiscordCommands();
                    services.AddCondition<RequireUserGuildPermissionCondition>();
                    services.AddCommandGroup<TestCommands>();
                    services.AddCommandGroup<ApplicationCommands>();
                    services.AddCommandGroup<ApplicationCommands.ViewCommands>();
                    
                    services.AddDiscordGateway(provider =>
                    {
                        var discordConfig = provider.GetRequiredService<DiscordSettings>();
                        return discordConfig.Token;
                    });
                    services.AddSingleton<DiscordService>();
                    services.AddSingleton<IHostedService, DiscordService>(serviceProvider =>
                        serviceProvider.GetRequiredService<DiscordService>());
                    services.AddDiscordCaching();
                });
    }
}