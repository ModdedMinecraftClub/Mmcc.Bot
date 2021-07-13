using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Common.Models.Settings;
using Mmcc.Bot.Core.Errors;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Mmcc.Bot.Events.Moderation.MemberApplications
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
            if (!ev.Attachments.HasValue
                || ev.Attachments.Value.Count == 0
                || !ev.ID.HasValue
                || !ev.Author.HasValue
                || ev.Author.Value.IsBot.HasValue && ev.Author.Value.IsBot.Value
                || !ev.GuildID.HasValue
                || !ev.ChannelID.HasValue
            )
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
                return new PropertyMissingOrNullError("Channel in which a potential application was sent has no name.");
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