using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Statics;
using Mmcc.Bot.Database;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.Infrastructure.Services;
using Mmcc.Bot.Protos;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Commands.ModerationActions
{
    /// <summary>
    /// Bans a user.
    /// </summary>
    public class Ban
    {
        /// <summary>
        /// Command to ban a user.
        /// </summary>
        public class Command : IRequest<Result>
        {
            /// <summary>
            /// Channel ID.
            /// </summary>
            public Snowflake ChannelId { get; set; }
            
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
            
            /// <summary>
            /// Expiry date. Set to <code>null</code> if permanent.
            /// </summary>
            public long? ExpiryDate { get; set; }
        }
        
        /// <inheritdoc />
        public class Handler : IRequestHandler<Command, Result>
        {
            private readonly BotContext _context;
            private readonly IDiscordRestGuildAPI _guildApi;
            private readonly IPolychatCommunicationService _pcs;
            private readonly IDiscordRestUserAPI _userApi;
            private readonly ColourPalette _colourPalette;
            private readonly ILogger<Handler> _logger;
            private readonly IDiscordRestChannelAPI _channelApi;

            /// <summary>
            /// Instantiates a new instance of <see cref="Handler"/> class.
            /// </summary>
            /// <param name="context">The DB context.</param>
            /// <param name="guildApi">The guild API.</param>
            /// <param name="pcs">The polychat communication service.</param>
            /// <param name="userApi">The user API.</param>
            /// <param name="colourPalette">The colour palette.</param>
            /// <param name="logger">The logger.</param>
            /// <param name="channelApi">The channel API.</param>
            public Handler(
                BotContext context,
                IDiscordRestGuildAPI guildApi,
                IPolychatCommunicationService pcs,
                IDiscordRestUserAPI userApi,
                ColourPalette colourPalette,
                ILogger<Handler> logger,
                IDiscordRestChannelAPI channelApi
            )
            {
                _context = context;
                _guildApi = guildApi;
                _pcs = pcs;
                _userApi = userApi;
                _colourPalette = colourPalette;
                _logger = logger;
                _channelApi = channelApi;
            }

            /// <inheritdoc />
            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var banModerationAction = new ModerationAction
                (
                    moderationActionType: ModerationActionType.Ban,
                    isActive: true,
                    reason: request.Reason,
                    date: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    userDiscordId: request.UserDiscordId?.Value,
                    userIgn: request.UserIgn,
                    expiryDate: request.ExpiryDate
                );

                if (request.UserIgn is not null)
                {
                    var protobufMessage = new GenericServerCommand
                    {
                        ServerId = "<all>",
                        Command = new GenericCommand
                        {
                            DefaultCommand = "ban",
                            DiscordCommandName = "ban",
                            DiscordChannelId = request.ChannelId.Value.ToString(),
                            Args = {request.UserIgn}
                        }
                    };
                    var sendProtobufMessageResult = await _pcs.SendProtobufMessage(protobufMessage);
                    if (!sendProtobufMessageResult.IsSuccess)
                    {
                        return new PolychatError(
                            "Could not communicate with polychat2's central server. Please see the logs.");
                    }
                }

                if (request.UserDiscordId is not null)
                {
                    var banResult = await _guildApi.CreateGuildBanAsync(
                        request.GuildId,
                        request.UserDiscordId.Value,
                        reason: request.Reason,
                        ct: cancellationToken
                    );

                    if (!banResult.IsSuccess)
                    {
                        return Result.FromError(banResult);
                    }

                    var embed = new Embed
                    {
                        Title = "You have been banned from Modded Minecraft Club.",
                        Colour = _colourPalette.Red,
                        Thumbnail = EmbedProperties.MmccLogoThumbnail,
                        Timestamp = DateTimeOffset.UtcNow,
                        Fields = new List<EmbedField>
                        {
                            new("Reason", request.Reason, false),
                            new(
                                "Expires at",
                                request.ExpiryDate is null
                                    ? "Permanent"
                                    : $"{DateTimeOffset.FromUnixTimeMilliseconds(request.ExpiryDate.Value).UtcDateTime} UTC",
                                false
                            )
                        }
                    };

                    var createDmResult = await _userApi.CreateDMAsync(request.UserDiscordId.Value, cancellationToken);
                    const string errMsg =
                        "Failed to send a DM notification to the user. It may be because they have blocked the bot or don't share any servers. This error can in most cases be ignored.";
                    if (!createDmResult.IsSuccess || createDmResult.Entity is null)
                    {
                        _logger.LogWarning(errMsg);
                    }
                    else
                    {
                        var sendDmResult = await _channelApi.CreateMessageAsync(createDmResult.Entity.ID, embed: embed,
                            ct: cancellationToken);
                        if (!sendDmResult.IsSuccess)
                        {
                            _logger.LogWarning(errMsg);
                        }
                    }
                }

                try
                {
                    await _context.AddAsync(banModerationAction, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    return e;
                }
                
                return Result.FromSuccess();
            }
        }
    }
}