using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.Common.Models.Settings;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Mmcc.Bot.EventResponders.Feedback;

/// <summary>
/// Responds to emojis that signify that feedback has been addressed, and protect them from being added by non-Staff.
/// </summary>
public class FeedbackAddressedResponder : IResponder<IMessageReactionAdd>
{
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly DiscordSettings _discordSettings;

    // emojis that only Staff can assign as they mean feedback has been addressed;
    private readonly string[] _protectedEmojis = {"✅", "❌"};

    public FeedbackAddressedResponder(IDiscordRestGuildAPI guildApi, IDiscordRestChannelAPI channelApi, DiscordSettings discordSettings)
    {
        _guildApi = guildApi;
        _channelApi = channelApi;
        _discordSettings = discordSettings;
    }

    public async Task<Result> RespondAsync(IMessageReactionAdd ev, CancellationToken ct = default)
    {
        if (ev.ChannelID.Value != _discordSettings.FeedbackChannelId
            || !ev.GuildID.HasValue
            || !ev.Member.HasValue
            || ev.Member.Value.User.HasValue && ev.Member.Value.User.Value.IsBot.HasValue
                                             && ev.Member.Value.User.Value.IsBot.Value
            || !ev.Emoji.Name.HasValue
            || ev.Emoji.Name.Value is null
            || !_protectedEmojis.Contains(ev.Emoji.Name.Value)
        )
        {
            return Result.FromSuccess();
        }

        var getChannelResult = await _channelApi.GetChannelAsync(ev.ChannelID, ct);
        if (!getChannelResult.IsSuccess)
        {
            return Result.FromError(getChannelResult);
        }

        var getGuildRolesResult = await _guildApi.GetGuildRolesAsync(ev.GuildID.Value, ct);
        if (!getGuildRolesResult.IsSuccess)
        {
            return Result.FromError(getGuildRolesResult);
        }

        var channel = getChannelResult.Entity;
        var guildRoles = getGuildRolesResult.Entity;
        var everyoneRole = guildRoles.FirstOrDefault(r => r.ID == ev.GuildID.Value);
            
        if (everyoneRole is null)
        {
            return new NotFoundError("No @everyone role found.");
        }
            
        var memberRoles = guildRoles.Where(r => ev.Member.Value.Roles.Contains(r.ID)).ToList();
        var computedPermissions = channel.PermissionOverwrites.HasValue
            ? DiscordPermissionSet.ComputePermissions(ev.UserID, everyoneRole, memberRoles,
                channel.PermissionOverwrites.Value)
            : DiscordPermissionSet.ComputePermissions(ev.UserID, everyoneRole, memberRoles);
        return computedPermissions.HasPermission(DiscordPermission.Administrator) 
               || computedPermissions.HasPermission(DiscordPermission.BanMembers)
            ? Result.FromSuccess()
            : await _channelApi.DeleteUserReactionAsync(ev.ChannelID, ev.MessageID, ev.Emoji.Name.Value, ev.UserID,
                ct);
    }
}