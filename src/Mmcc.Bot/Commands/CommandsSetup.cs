using Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.Commands.Minecraft;
using Mmcc.Bot.Commands.Minecraft.Restarts;
using Mmcc.Bot.Commands.MmccInfo;
using Mmcc.Bot.Commands.Moderation;
using Mmcc.Bot.Commands.Moderation.Bans;
using Mmcc.Bot.Commands.Moderation.MemberApplications;
using Mmcc.Bot.Commands.Moderation.PlayerInfo;
using Mmcc.Bot.Commands.Moderation.Warns;
using Mmcc.Bot.Commands.Tags.Management;
using Mmcc.Bot.Commands.Tags.Usage;
using Mmcc.Bot.Features.Diagnostics;
using Mmcc.Bot.Features.Guilds;
using Mmcc.Bot.Features.Help;
using Mmcc.Bot.Features.MmccInfo;
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

        services.AddCommandTree()
            // add core commands;
            .WithCommandGroup<HelpCommands>()
            .WithCommandGroup<GuildCommands>()
            .WithCommandGroup<MmccInfoCommands>()
            // add tags;
            .WithCommandGroup<TagsManagementCommands>()
            .WithCommandGroup<TagsUsageCommands>()
            // add diagnostics;
            .WithCommandGroup<DiagnosticsCommands>()
            // add in-game;
            .WithCommandGroup<MinecraftServersCommands>()
            .WithCommandGroup<MinecraftAutoRestartsCommands>()
            // add member apps;
            .WithCommandGroup<MemberApplicationsCommands>()
            // add moderation;
            .WithCommandGroup<GeneralModerationCommands>()
            .WithCommandGroup<PlayerInfoCommands>()
            .WithCommandGroup<BanCommands>()
            .WithCommandGroup<WarnCommands>()
            // and build it;
            .Finish();
        
        return services;
    }
}