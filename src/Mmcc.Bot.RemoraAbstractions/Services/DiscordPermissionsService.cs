using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Services
{
    /// <summary>
    /// Service that authorises Discord users.
    /// </summary>
    public interface IDiscordPermissionsService
    {
        /// <summary>
        /// Checks if a user has a required <see cref="DiscordPermission"/>.
        /// </summary>
        /// <param name="permission">The required permission.</param>
        /// <param name="channelId">The channel ID (used to get channel overrides).</param>
        /// <param name="userToCheck">The user to check.</param>
        /// <param name="ct">The <see cref="CancellationToken"/>.</param>
        /// <returns>Result of the asynchronous operation.</returns>
        ValueTask<Result> CheckHasRequiredPermission(
            DiscordPermission permission,
            Snowflake channelId,
            IUser userToCheck,
            CancellationToken ct
        );
    }
    
    /// <inheritdoc />
    public class DiscordPermissionsService : IDiscordPermissionsService
    {
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestGuildAPI _guildApi;

        /// <summary>
        /// Instantiates a new instance of <see cref="DiscordPermissionsService"/>.
        /// </summary>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="guildApi">The guild API.</param>
        public DiscordPermissionsService(IDiscordRestChannelAPI channelApi, IDiscordRestGuildAPI guildApi)
        {
            _channelApi = channelApi;
            _guildApi = guildApi;
        }

        /// <inheritdoc />
        public async ValueTask<Result> CheckHasRequiredPermission(
            DiscordPermission permission,
            Snowflake channelId,
            IUser userToCheck,
            CancellationToken ct = default
        )
        {
            var getChannel = await _channelApi.GetChannelAsync(channelId, ct);
            if (!getChannel.IsSuccess)
            {
                return Result.FromError(getChannel);
            }

            var channel = getChannel.Entity;
            if (!channel.GuildID.HasValue)
            {
                return new ConditionNotSatisfiedError(
                    "Command requires a guild permission but was executed outside of a guild.");
            }

            var guildId = channel.GuildID.Value;

            var getGuildMember = await _guildApi.GetGuildMemberAsync(guildId, userToCheck.ID, ct);
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
                return new NotFoundError("No @everyone role found.");
            }

            var user = getGuildMember.Entity;
            if (user is null)
            {
                return new NotFoundError("Executing user not found");
            }

            var getGuild = await _guildApi.GetGuildAsync(guildId, ct: ct);
            if (!getGuild.IsSuccess)
            {
                return Result.FromError(getGuild);
            }

            var guildOwnerId = getGuild.Entity.OwnerID;

            // succeed if the user is the Owner of the guild
            if (guildOwnerId.Equals(userToCheck.ID))
            {
                return Result.FromSuccess();
            }

            var memberRoles = guildRoles.Where(r => user.Roles.Contains(r.ID)).ToList();
            var computedPermissions = channel.PermissionOverwrites switch
            {
                { HasValue: true, Value: { } overwrites } => DiscordPermissionSet.ComputePermissions(
                    userToCheck.ID,
                    everyoneRole,
                    memberRoles,
                    overwrites
                ),

                _ => DiscordPermissionSet.ComputePermissions(
                    userToCheck.ID,
                    everyoneRole,
                    memberRoles
                )
            };

            // succeed if the user is an Administrator of the guild
            if (computedPermissions.HasPermission(DiscordPermission.Administrator))
            {
                return Result.FromSuccess();
            }

            var hasPermission = computedPermissions.HasPermission(permission);
            return !hasPermission
                ? new ConditionNotSatisfiedError(
                    $"Guild User requesting the command does not have the required {permission.ToString()} permission")
                : Result.FromSuccess();
        }
    }
}