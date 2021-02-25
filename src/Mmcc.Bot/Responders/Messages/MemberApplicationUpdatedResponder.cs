using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Core.Models.Settings;
using Mmcc.Bot.Infrastructure.Commands.MemberApplications;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Mmcc.Bot.Responders.Messages
{
    /// <summary>
    /// Responds to a member application, that is a message that contains a screenshot and that was sent in the #member-apps channel, being updated.
    /// </summary>
    public class MemberApplicationUpdatedResponder : IResponder<IMessageUpdate>
    {
        private readonly ILogger<MemberApplicationUpdatedResponder> _logger;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IMediator _mediator;
        private readonly DiscordSettings _discordSettings;

        /// <summary>
        /// Instantiates a new instance of the <see cref="MemberApplicationUpdatedResponder"/> class.
        /// </summary>
        /// <param name="logger">The logging instance.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="discordSettings">The Discord settings.</param>
        public MemberApplicationUpdatedResponder(
            ILogger<MemberApplicationUpdatedResponder> logger,
            IDiscordRestChannelAPI channelApi,
            IMediator mediator,
            DiscordSettings discordSettings
            )
        {
            _logger = logger;
            _channelApi = channelApi;
            _mediator = mediator;
            _discordSettings = discordSettings;
        }

        /// <inheritdoc />
        public async Task<Result> RespondAsync(IMessageUpdate ev, CancellationToken ct = default)
        {
            // return if the message has no screenshot;
            if (!ev.Attachments.HasValue)
            {
                return Result.FromSuccess();
            }
            if (ev.Attachments.Value.Count == 0)
            {
                return Result.FromSuccess();
            }
            
            // return if no ID;
            if (!ev.ID.HasValue)
            {
                return Result.FromSuccess();
            }
            
            // return if no author;
            if (!ev.Author.HasValue)
            {
                return Result.FromSuccess();
            }
            
            // return if bot
            if (ev.Author.Value.IsBot.HasValue)
            {
                if (ev.Author.Value.IsBot.Value)
                {
                    return Result.FromSuccess();
                }
            }
            
            // return if not in a guild;
            if (!ev.GuildID.HasValue)
            {
                return Result.FromSuccess();
            }

            // return if no channel;
            if (!ev.ChannelID.HasValue)
            {
                return Result.FromSuccess();
            }
            
            var getChannelNameResult = await _channelApi.GetChannelAsync(ev.ChannelID.Value, ct);
            if (!getChannelNameResult.IsSuccess)
            {
                return Result.FromError(getChannelNameResult);
            }

            var channelName = getChannelNameResult.Entity.Name;
            if (!channelName.HasValue)
            {
                return new GenericError("Channel in which a potential application was sent has no name.");
            }
            
            // return if the message isn't in #member-apps;
            if (!channelName.Value.Equals(_discordSettings.ChannelNames.MemberApps))
            {
                return Result.FromSuccess();
            }

            var res = await _mediator.Send(
                new UpdateFromDiscordMessage.Command{DiscordMessageUpdatedEvent = ev}, ct);

            if (res.IsSuccess)
            {
                _logger.LogInformation($"Updated the application for the message: {ev.ID.Value}");
            }

            return res;
        }
    }
}