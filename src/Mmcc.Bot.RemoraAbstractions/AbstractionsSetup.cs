using Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.RemoraAbstractions.Conditions;
using Mmcc.Bot.RemoraAbstractions.Parsers;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Commands.Extensions;

namespace Mmcc.Bot.RemoraAbstractions;

/// <summary>
/// Extension methods that register Mmcc.Bot.RemoraAbstractions with the service collection.
/// </summary>
public static class AbstractionsSetup
{
    /// <summary>
    /// Registers Mmcc.Bot.RemoraAbstractions classes with the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddRemoraAbstractions(this IServiceCollection services)
    {
        services.AddScoped<IHelpService, HelpService>();
        services.AddScoped<IDmSender, DmSender>();
        services.AddScoped<IDiscordPermissionsService, DiscordPermissionsService>();
        services.AddScoped<ICommandResponder, CommandResponder>();
        services.AddScoped<IInteractionResponder, InteractionResponder>();
            
        services.AddCondition<RequireGuildCondition>();
        services.AddCondition<RequireUserGuildPermissionCondition>();

        services.AddParser<TimeSpanParser>();
        services.AddParser<ExpiryDateParser>();

        return services;
    }
}