using Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.Interactions.Moderation.MemberApplications;
using Remora.Discord.Interactivity.Extensions;

namespace Mmcc.Bot.Interactions;

/// <summary>
/// Extension methods that register interactive functionality with the service collection.
/// </summary>
public static class InteractionsSetup
{
    /// <summary>
    /// Registers interactive functionality with the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddInteractions(this IServiceCollection services)
    {
        services.AddInteractivity();

        services.AddInteractionGroup<MemberApplicationsInteractions>();
        
        return services;
    }
}