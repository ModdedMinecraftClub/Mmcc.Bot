using System;
using System.IO;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mmcc.Bot.Common.Models.Settings;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;

namespace Mmcc.Bot.CoreSetup;

public static class LoggerSetup
{
    public static void ConfigureLogger(HostBuilderContext _, IServiceProvider provider,
        LoggerConfiguration loggerConfiguration)
    {
        var isDevelopment = provider.GetRequiredService<IHostEnvironment>().IsDevelopment();

        loggerConfiguration.MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System.Net.Http.HttpClient",
                isDevelopment 
                    ? LogEventLevel.Information 
                    : LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:dd/MM/yyyy HH:mm:ss:fff} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                theme: isDevelopment ? null : AnsiConsoleTheme.Literate)
            .WriteTo.File(
                new CompactJsonFormatter(),
                Path.Combine("logs", "log.clef"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                levelSwitch: new LoggingLevelSwitch(LogEventLevel.Warning));

        var azureSettings = provider.GetRequiredService<AzureLoggingSettings>();

        // ReSharper disable once InvertIf
        if (azureSettings.Enabled)
        {
            var appInsightsConfig = provider.GetRequiredService<TelemetryConfiguration>();

            loggerConfiguration
                .WriteTo
                .ApplicationInsights(appInsightsConfig, TelemetryConverter.Traces, LogEventLevel.Warning);
        }
    }
}