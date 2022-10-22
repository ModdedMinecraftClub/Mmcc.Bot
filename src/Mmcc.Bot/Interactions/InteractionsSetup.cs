using Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.Interactions.Moderation.MemberApplications;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Interactivity;
using Remora.Discord.Interactivity.Extensions;
using Remora.Extensions.Options.Immutable;

using InteractivityResponder = Mmcc.Bot.EventResponders.Interactions.InteractivityResponder;

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
        //services.AddInteractivity(); <= default Remora interactivity. We don't use because we have a custom pipeline
        
        services.AddMemoryCache();
        
        services.AddInteractionGroup<MemberApplicationsInteractions>();

        return services;
    }
}