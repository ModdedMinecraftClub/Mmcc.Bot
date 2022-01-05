using Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.Commands.Core;
using Mmcc.Bot.Commands.Core.Help;
using Mmcc.Bot.Commands.Diagnostics;
using Mmcc.Bot.Commands.Guilds;
using Mmcc.Bot.Commands.Minecraft;
using Mmcc.Bot.Commands.Moderation;
using Mmcc.Bot.Commands.Moderation.Bans;
using Mmcc.Bot.Commands.Moderation.MemberApplications;
using Mmcc.Bot.Commands.Moderation.PlayerInfo;
using Mmcc.Bot.Commands.Moderation.Warns;
using Mmcc.Bot.Commands.Tags.Management;
using Mmcc.Bot.Commands.Tags.Usage;
using Remora.Commands.Extensions;
using Remora.Discord.Commands.Extensions;

namespace Mmcc.Bot.Commands;

/// <summary>
/// Extension methods that register commands with the service collection.
/// </summary>
public static class CommandsSetup
{
    /// <summary>
    /// Registers commands with the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddBotCommands(this IServiceCollection services)
    {
        services.AddDiscordCommands();

        // core commands;
        services.AddCommandGroup<HelpCommands>();
        services.AddCommandGroup<GuildCommands>();
        services.AddCommandGroup<MmccInfoCommands>();
                    
        // tags;
        services.AddCommandGroup<TagsManagementCommands>();
        services.AddCommandGroup<TagsUsageCommands>();

        // diagnostics;
        services.AddCommandGroup<DiagnosticsCommands>();
                    
        // in game;
        services.AddCommandGroup<MinecraftServersCommands>();
        services.AddCommandGroup<MinecraftAutoRestartsCommands>();
                    
        // member apps;
        services.AddCommandGroup<MemberApplicationsCommands>();

        // moderation;
        services.AddCommandGroup<GeneralModerationCommands>();
        services.AddCommandGroup<PlayerInfoCommands>();
        services.AddCommandGroup<BanCommands>();
        services.AddCommandGroup<WarnCommands>();

        return services;
    }
}