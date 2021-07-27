using System;
using System.IO;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Behaviours;
using Mmcc.Bot.Caching;
using Mmcc.Bot.Commands;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Models.Settings;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Settings;
using Mmcc.Bot.EventResponders;
using Mmcc.Bot.EventResponders.Moderation.MemberApplications;
using Mmcc.Bot.Hosting;
using Mmcc.Bot.Middleware;
using Mmcc.Bot.Mojang;
using Mmcc.Bot.Polychat;
using Mmcc.Bot.RemoraAbstractions;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Hosting.Services;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

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
                    // config;
                    services.ConfigureBot(hostContext);
                    
                    // db;
                    services.AddBotDatabaseContext();
                    
                    services.AddSingleton<IColourPalette, TailwindColourPalette>();
                    
                    // FluentValidation;
                    services.AddValidatorsFromAssemblyContaining<Program>();
                    services.AddValidatorsFromAssemblyContaining<DiscordSettings>();
                    services.AddValidatorsFromAssemblyContaining<MySqlSettingsValidator>();
                    
                    // MediatR;
                    services.AddMediatR(typeof(CreateFromDiscordMessage), typeof(TcpRequest<>));
                    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
                    
                    // Mmcc.Bot.X projects;
                    services.AddMmccCaching();
                    services.AddMojangApi();
                    services.AddPolychat(hostContext.Configuration.GetSection("Ssmp"));
                    
                    // Remora.Discord bot setup;
                    services.AddRemoraAbstractions();
                    services.AddBotMiddlewares();
                    services.AddBotCommands();
                    services.AddBotGatewayEventResponders();
                    services.AddDiscordGateway(provider =>
                    {
                        var discordConfig = provider.GetRequiredService<DiscordSettings>();
                        return discordConfig.Token;
                    });
                    services.AddDiscordCaching();
                    services.AddHostedService<DiscordService>();
                    services.AddBotBackgroundServices();
                })
                .UseDefaultServiceProvider(options => options.ValidateScopes = true)
                .UseSerilog();
    }
}