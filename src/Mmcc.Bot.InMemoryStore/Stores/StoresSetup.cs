using Microsoft.Extensions.DependencyInjection;

namespace Mmcc.Bot.InMemoryStore.Stores;

/// <summary>
/// Extension methods that register stores in Mmcc.Bot.InMemoryStore.Stores with the service collection.
/// </summary>
public static class StoresSetup
{
    /// <summary>
    /// Registers stores in Mmcc.Bot.InMemoryStore.Store classes with the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddInMemoryStores(this IServiceCollection services) =>
        services.AddSingleton<IMessageMemberAppContextStore, MessageMemberAppContextStore>();
}