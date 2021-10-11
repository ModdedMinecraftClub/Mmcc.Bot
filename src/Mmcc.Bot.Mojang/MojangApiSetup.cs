using Microsoft.Extensions.DependencyInjection;

namespace Mmcc.Bot.Mojang;

/// <summary>
/// Extension methods that register Mmcc.Bot.Mojang with the service collection.
/// </summary>
public static class MojangApiSetup
{
    /// <summary>
    /// Registers Mmcc.Bot.Mojang classes with the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddMojangApi(this IServiceCollection services) =>
        services.AddScoped<IMojangApiService, MojangApiService>();
}