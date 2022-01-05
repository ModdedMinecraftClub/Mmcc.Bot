using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Statics;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.Polychat;
using Mmcc.Bot.Polychat.Services;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;

namespace Mmcc.Bot.Commands.Moderation.Warns;

/// <summary>
/// Warns a user.
/// </summary>
public class Warn
{
    /// <summary>
    /// Command to warn a user.
    /// </summary>
    public class Command : IRequest<Result<ModerationAction>>
    {
        /// <summary>
        /// Guild ID.
        /// </summary>
        public Snowflake GuildId { get; set; }
            
        /// <summary>
        /// User's Discord ID. Set to <code>null</code> if not associated with a Discord user.
        /// </summary>
        public Snowflake? UserDiscordId { get; set; }
            
        /// <summary>
        /// User's IGN. Set to <code>null</code> if not associated with a MC user.
        /// </summary>
        public string? UserIgn { get; set; }
            
        /// <summary>
        /// Reason
        /// </summary>
        public string Reason { get; set; } = null!;
    }

    /// <summary>
    /// Validates the <see cref="Command"/>.
    /// </summary>
    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.GuildId)
                .NotNull();

            RuleFor(c => c.UserIgn)
                .NotEmpty().When(c => c.UserIgn is not null);

            RuleFor(c => c.Reason)
                .NotEmpty();
        }
    }

    /// <inheritdoc />
    public class Handler : IRequestHandler<Command, Result<ModerationAction>>
    {
        private readonly BotContext _context;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly IPolychatService _ps;
        private readonly IDiscordRestUserAPI _userApi;
        private readonly IColourPalette _colourPalette;
        private readonly ILogger<Handler> _logger;
        private readonly IDiscordRestChannelAPI _channelApi;
            
        /// <summary>
        /// Instantiates a new instance of <see cref="Handler"/> class.
        /// </summary>
        /// <param name="context">The DB context.</param>
        /// <param name="guildApi">The guild API.</param>
        /// <param name="ps">The polychat service.</param>
        /// <param name="userApi">The user API.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="channelApi">The channel API.</param>
        public Handler(
            BotContext context,
            IDiscordRestGuildAPI guildApi,
            IPolychatService ps,
            IDiscordRestUserAPI userApi,
            IColourPalette colourPalette,
            ILogger<Handler> logger,
            IDiscordRestChannelAPI channelApi
        )
        {
            _context = context;
            _guildApi = guildApi;
            _ps = ps;
            _userApi = userApi;
            _colourPalette = colourPalette;
            _logger = logger;
            _channelApi = channelApi;
        }
            
        /// <inheritdoc />
        public async Task<Result<ModerationAction>> Handle(Command request, CancellationToken cancellationToken)
        {
            var warnModerationAction = new ModerationAction
            (
                moderationActionType: ModerationActionType.Warn,
                guildId: request.GuildId.Value,
                isActive: true,
                reason: request.Reason,
                date: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                userDiscordId: request.UserDiscordId?.Value,
                userIgn: request.UserIgn,
                expiryDate: null
            );

            if (request.UserIgn is not null)
            {
                var protobufMessage = new ChatMessage
                {
                    ServerId = "MMCC",
                    Message = $"You have been warned, @{request.UserIgn}. Reason: {request.Reason}",
                    MessageOffset = 5
                };
                await _ps.BroadcastMessage(protobufMessage);
            }

            if (request.UserDiscordId is not null)
            {
                var guildResult = await _guildApi.GetGuildAsync(request.GuildId, ct: cancellationToken);

                if (!guildResult.IsSuccess)
                {
                    return Result<ModerationAction>.FromError(guildResult.Error);
                }

                var guildName = guildResult.Entity.Name;
                var embed = new Embed
                {
                    Title = $"You have been warned in {guildName}.",
                    Colour = _colourPalette.Yellow,
                    Thumbnail = EmbedProperties.MmccLogoThumbnail,
                    Timestamp = DateTimeOffset.UtcNow,
                    Fields = new List<EmbedField>
                    {
                        new("Reason", request.Reason, false)
                    }
                };

                var createDmResult = await _userApi.CreateDMAsync(request.UserDiscordId.Value, cancellationToken);
                const string errMsg =
                    "Failed to send a DM notification to the user. It may be because they have blocked the bot. This error can in most cases be ignored.";
                if (!createDmResult.IsSuccess || createDmResult.Entity is null)
                {
                    _logger.LogWarning(errMsg);
                }
                else
                {
                    var sendDmResult = await _channelApi.CreateMessageAsync(createDmResult.Entity.ID,
                        embeds: new[] { embed },
                        ct: cancellationToken);
                    if (!sendDmResult.IsSuccess)
                    {
                        _logger.LogWarning(errMsg);
                    }
                }
            }

            try
            {
                await _context.AddAsync(warnModerationAction, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception e)
            {
                return e;
            }
                
            return warnModerationAction;
        }
    }
}