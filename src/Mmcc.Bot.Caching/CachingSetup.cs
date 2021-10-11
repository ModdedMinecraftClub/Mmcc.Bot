using Microsoft.Extensions.DependencyInjection;

namespace Mmcc.Bot.Caching;

/// <summary>
/// Extension methods that register Mmcc.Bot.Caching with the service collection.
/// </summary>
public static class CachingSetup
{
    /// <summary>
    /// Registers Mmcc.Bot.Caching classes with the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddMmccCaching(this IServiceCollection services) =>
        services.AddSingleton<IButtonHandlerRepository, ButtonHandlerRepository>();
}