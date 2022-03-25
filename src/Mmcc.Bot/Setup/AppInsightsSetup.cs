using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mmcc.Bot.Common.Extensions.Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.Common.Models.Settings;

namespace Mmcc.Bot.Setup;

public static class AppInsightsSetup
{
    public static IServiceCollection AddAppInsights(this IServiceCollection services, HostBuilderContext hostContext)
    {
        var azureConfig = hostContext.Configuration.GetSection("AzureLogging");

        services.AddConfigWithValidation<AzureLoggingSettings, AzureLoggingSettingsValidator>(azureConfig);

        var azureLoggingEnabled = azureConfig.GetValue<bool>("Enabled");
        var instrumentationKey = azureConfig.GetValue<string>("InstrumentationKey");

        if (!azureLoggingEnabled)
        {
            services.AddApplicationInsightsTelemetryWorkerService(o =>
            {
                o.InstrumentationKey = instrumentationKey;
                o.EnableDiagnosticsTelemetryModule = false;
                o.EnableDependencyTrackingTelemetryModule = false;
                o.EnableEventCounterCollectionModule = false;
                o.EnablePerformanceCounterCollectionModule = false;
                o.EnableAppServicesHeartbeatTelemetryModule = false;
                o.EnableAzureInstanceMetadataTelemetryModule = false;
            });
        }
        else
        {
            services.AddApplicationInsightsTelemetryWorkerService(instrumentationKey);
        }
        
        return services;
    }
}