using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Common.Extensions.Remora.Discord.API.Abstractions.Rest;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Models.Settings;
using Mmcc.Bot.Common.Statics;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.Polychat;
using Mmcc.Bot.Polychat.Services;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Errors;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;

namespace Mmcc.Bot.Commands.Moderation.Bans;

/// <summary>
/// Unbans a user.
/// </summary>
public class Unban
{
    public class Command : IRequest<Result<ModerationAction>>
    {
        public ModerationAction ModerationAction { get; set; } = null!;
    }
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.ModerationAction)
                .NotNull();
        }
    }
        
    /// <inheritdoc />
    public class Handler : IRequestHandler<Command, Result<ModerationAction>>
    {
        private readonly BotContext _context;
        private readonly IPolychatService _ps;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly IDiscordRestUserAPI _userApi;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IColourPalette _colourPalette;
        private readonly ILogger<Handler> _logger;
        private readonly DiscordSettings _discordSettings;
        
        public Handler(
            BotContext context,
            IPolychatService ps,
            IDiscordRestGuildAPI guildApi,
            IDiscordRestUserAPI userApi,
            IDiscordRestChannelAPI channelApi,
            IColourPalette colourPalette,
            ILogger<Handler> logger, 
            DiscordSettings discordSettings
        )
        {
            _context = context;
            _ps = ps;
            _guildApi = guildApi;
            _userApi = userApi;
            _channelApi = channelApi;
            _colourPalette = colourPalette;
            _logger = logger;
            _discordSettings = discordSettings;
        }

        /// <inheritdoc />
        public async Task<Result<ModerationAction>> Handle(Command request, CancellationToken cancellationToken)
        {
            var ma = request.ModerationAction;
            if (ma.ModerationActionType != ModerationActionType.Ban)
                return new UnsupportedArgumentError($"Wrong moderation action type. Expected: {ModerationActionType.Ban}, got: {ma.ModerationActionType}");
            if (!ma.IsActive)
                return new UnsupportedArgumentError("Moderation action is already inactive.");

            
            if (ma.UserIgn is not null)
            {
                var getLogsChannel = await _guildApi.FindGuildChannelByName(new(ma.GuildId), _discordSettings.ChannelNames.ModerationLogs);
                if (!getLogsChannel.IsSuccess)
                {
                    _logger.LogError("An error has occurred while obtaining logs channel.");
                }

                var proto = new GenericCommand
                {
                    DiscordCommandName = "exec",
                    DefaultCommand = "$args",
                    Args = { "pardon", ma.UserIgn },
                    DiscordChannelId = getLogsChannel.Entity?.ID.ToString()
                };
                await _ps.BroadcastMessage(proto);
            }

            if (request.ModerationAction.UserDiscordId is not null)
            {
                var userDiscordIdSnowflake = new Snowflake(request.ModerationAction.UserDiscordId.Value);
                var banResult = await _guildApi.RemoveGuildBanAsync(
                    new(request.ModerationAction.GuildId),
                    new(request.ModerationAction.UserDiscordId.Value),
                    new(),
                    cancellationToken
                );

                if (!banResult.IsSuccess)
                {
                    return Result<ModerationAction>.FromError(banResult.Error);
                }

                var embed = new Embed
                {
                    Title = "You have been unbanned from Modded Minecraft Club.",
                    Colour = _colourPalette.Green,
                    Thumbnail = EmbedProperties.MmccLogoThumbnail
                };

                var createDmResult = await _userApi.CreateDMAsync(userDiscordIdSnowflake, cancellationToken);
                const string warningMsg =
                    "Failed to send a DM notification to the user. It may be because they have blocked the bot or don't share any servers. This warning can in most cases be ignored.";
                if (!createDmResult.IsSuccess)
                {
                    _logger.LogWarning(warningMsg);
                }
                else
                {
                    var sendDmResult = await _channelApi.CreateMessageAsync(createDmResult.Entity.ID,
                        embeds: new[] { embed },
                        ct: cancellationToken);
                    if (!sendDmResult.IsSuccess)
                    {
                        _logger.LogWarning(warningMsg);
                    }
                }
            }

            try
            {
                ma.IsActive = false;
                ma.ExpiryDate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                return e;
            }
                
            return ma;
        }
    }
}