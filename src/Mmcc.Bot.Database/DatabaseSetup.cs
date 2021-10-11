using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.Database.Settings;

namespace Mmcc.Bot.Database;

/// <summary>
/// Extension methods that configure and register the DB context with the service provider.
/// </summary>
public static class DatabaseSetup
{
    /// <summary>
    /// Configures and registers the DB context with the service provider.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddBotDatabaseContext(this IServiceCollection services) =>
        services.AddDbContext<BotContext>((provider, options) =>
        {
            var dbConfig = provider.GetRequiredService<MySqlSettings>();
            var connString = dbConfig.ConnectionString;
            var serverVersion = ServerVersion.Parse(dbConfig.MySqlVersionString);
            var retryAmount = dbConfig.RetryAmount;

            options.UseMySql(connString, serverVersion,
                contextOptions => contextOptions.EnableRetryOnFailure(retryAmount));
        });
}