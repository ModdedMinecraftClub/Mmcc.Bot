using System;
using Hangfire;
using Hangfire.Storage.MySql;
using Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.Database.Settings;
using Mmcc.Bot.Polychat.Jobs;

namespace Mmcc.Bot;

public static class HangfireSetup
{
    public static IServiceCollection AddHangfire(this IServiceCollection services)
    {
        services.AddHangfire((provider, configuration) => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseStorage(
                new MySqlStorage(provider.GetRequiredService<MySqlSettings>().ConnectionString,
                    new MySqlStorageOptions
                    {
                        QueuePollInterval = TimeSpan.FromSeconds(15),
                        JobExpirationCheckInterval = TimeSpan.FromMinutes(20),
                        CountersAggregateInterval = TimeSpan.FromMinutes(5),
                        PrepareSchemaIfNecessary = true,
                        TransactionTimeout = TimeSpan.FromMinutes(1),
                        TablesPrefix = "__Hangfire"
                    })
            ));
        
        services.AddHangfireServer(o =>
        {
            o.Queues = new[] {PolychatJobQueues.Default, PolychatJobQueues.ServerRestarts};
        });

        return services;
    }
}