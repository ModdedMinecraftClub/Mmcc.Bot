using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Remora.Commands.Conditions;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Conditions
{
    /// <summary>
    /// Checks required Guild permissions before allowing execution.
    ///
    /// <remarks>Fails if the command is executed outside of a Guild. It should be used together with <see cref=""./></remarks>
    /// </summary>
    public class RequireUserGuildPermissionCondition : ICondition<RequireUserGuildPermissionAttribute>
    {
        private readonly ICommandContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestGuildAPI _guildApi;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequireUserGuildPermissionCondition"/> class.
        /// </summary>
        /// <param name="context">The command context.</param>
        /// <param name="guildApi">The guild API.</param>
        /// <param name="channelApi">The channel API.</param>
        public RequireUserGuildPermissionCondition(ICommandContext context, IDiscordRestGuildAPI guildApi, IDiscordRestChannelAPI channelApi)
        {
            _context = context;
            _guildApi = guildApi;
            _channelApi = channelApi;
        }

        /// <inheritdoc />
        public async ValueTask<Result> CheckAsync(RequireUserGuildPermissionAttribute attribute, CancellationToken ct)
        {
            var getChannel = await _channelApi.GetChannelAsync(_context.ChannelID, ct);
            if (!getChannel.IsSuccess)
            {
                return Result.FromError(getChannel);
            }
            var channel = getChannel.Entity;
            if (!channel.GuildID.HasValue)
            {
                return new ConditionNotSatisfiedError("Command requires a guild permission but was executed outside of a guild.");
            }

            var guildId = channel.GuildID.Value;

            var getGuildMember = await _guildApi.GetGuildMemberAsync(guildId, _context.User.ID, ct);
            if (!getGuildMember.IsSuccess)
            {
                return Result.FromError(getGuildMember);
            }

            var getGuildRoles = await _guildApi.GetGuildRolesAsync(guildId, ct);
            if (!getGuildRoles.IsSuccess)
            {
                return Result.FromError(getGuildRoles);
            }

            var guildRoles = getGuildRoles.Entity;
            var everyoneRole = guildRoles.FirstOrDefault(r => r.Name.Equals("@everyone"));
            if (everyoneRole is null)
            {
                return new GenericError("No @everyone role found.");
            }

            var user = getGuildMember.Entity;
            if (user is null)
            {
                return new GenericError("Executing user not found");
            }

            var getGuild = await _guildApi.GetGuildAsync(guildId, ct: ct);
            if (!getGuild.IsSuccess)
            {
                return Result.FromError(getGuild);
            }
            var guildOwnerId = getGuild.Entity.OwnerID;

            // succeed if the user is the Owner of the guild
            if (guildOwnerId.Equals(_context.User.ID))
            {
                return Result.FromSuccess();
            }

            var memberRoles = guildRoles.Where(r => user.Roles.Contains(r.ID)).ToList();
            IDiscordPermissionSet computedPermissions;
            if (channel.PermissionOverwrites.HasValue)
            {
                computedPermissions = DiscordPermissionSet.ComputePermissions(
                    _context.User.ID, everyoneRole, memberRoles, channel.PermissionOverwrites.Value
                );
            }
            else
            {
                computedPermissions = DiscordPermissionSet.ComputePermissions(
                    _context.User.ID, everyoneRole, memberRoles
                );
            }

            // succeed if the user is an Administrator of the guild
            if (computedPermissions.HasPermission(DiscordPermission.Administrator))
            {
                return Result.FromSuccess();
            }

            var hasPermission = computedPermissions.HasPermission(attribute.Permission);
            return !hasPermission
                ? new ConditionNotSatisfiedError($"Guild User requesting the command does not have the required {attribute.Permission.ToString()} permission")
                : Result.FromSuccess();
        }
    }
}