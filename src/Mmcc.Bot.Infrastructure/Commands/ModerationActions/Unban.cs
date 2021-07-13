using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Statics;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.Polychat;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Errors;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Commands.ModerationActions
{
    /// <summary>
    /// Unbans a user.
    /// </summary>
    public class Unban
    {
        /// <summary>
        /// Command to unban a user.
        /// </summary>
        public class Command : IRequest<Result<ModerationAction>>
        {
            /// <summary>
            /// Moderation action.
            /// </summary>
            public ModerationAction ModerationAction { get; set; } = null!;
            
            /// <summary>
            /// ID of the channel to which polychat2 will send the confirmation message.
            /// </summary>
            public Snowflake ChannelId { get; set; }
        }

        /// <summary>
        /// Validates the <see cref="Command"/>.
        /// </summary>
        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.ModerationAction)
                    .NotNull();

                RuleFor(c => c.ChannelId)
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

            /// <summary>
            /// Instantiates a new instance of <see cref="Handler"/> class.
            /// </summary>
            /// <param name="context">The DB context.</param>
            /// <param name="ps">The polychat service.</param>
            /// <param name="guildApi">The guild API.</param>
            /// <param name="userApi">The user API.</param>
            /// <param name="channelApi">The channel API.</param>
            /// <param name="colourPalette">The colour palette.</param>
            /// <param name="logger">The logger.</param>
            public Handler(
                BotContext context,
                IPolychatService ps,
                IDiscordRestGuildAPI guildApi,
                IDiscordRestUserAPI userApi,
                IDiscordRestChannelAPI channelApi,
                IColourPalette colourPalette,
                ILogger<Handler> logger
            )
            {
                _context = context;
                _ps = ps;
                _guildApi = guildApi;
                _userApi = userApi;
                _channelApi = channelApi;
                _colourPalette = colourPalette;
                _logger = logger;
            }

            /// <inheritdoc />
            public async Task<Result<ModerationAction>> Handle(Command request, CancellationToken cancellationToken)
            {
                var ma = request.ModerationAction;
                if (ma.ModerationActionType != ModerationActionType.Ban)
                    return new UnsupportedArgumentError(
                        $"Wrong moderation action type. Expected: {ModerationActionType.Ban}, got: {ma.ModerationActionType}"); 
                //if (!ma.IsActive) return new ValidationError("Moderation action is already inactive.");

                if (ma.UserIgn is not null)
                {
                    var proto = new GenericCommand
                    {
                        DefaultCommand = "ban",
                        DiscordCommandName = "ban",
                        DiscordChannelId = request.ChannelId.ToString(),
                        Args = {request.ModerationAction.UserIgn}
                    };
                    await _ps.BroadcastMessage(proto);
                }

                if (request.ModerationAction.UserDiscordId is not null)
                {
                    var userDiscordIdSnowflake = new Snowflake(request.ModerationAction.UserDiscordId.Value);
                    var banResult = await _guildApi.RemoveGuildBanAsync(
                        new(request.ModerationAction.GuildId),
                        new(request.ModerationAction.UserDiscordId.Value),
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
                    if (!createDmResult.IsSuccess || createDmResult.Entity is null)
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
}