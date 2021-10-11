using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Common.Errors;
using Mmcc.Bot.Common.Models.Settings;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Mmcc.Bot.EventResponders.Moderation.MemberApplications;

/// <summary>
/// Responds to a creation of a member application, that is a message that contains a screenshot and that was sent in the #member-apps channel.
/// </summary>
public class MemberApplicationCreatedResponder : IResponder<IMessageCreate>
{
    private readonly ILogger<MemberApplicationCreatedResponder> _logger;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IMediator _mediator;
    private readonly DiscordSettings _discordSettings;

    /// <summary>
    /// Instantiates a new instance of the <see cref="MemberApplicationCreatedResponder"/> class.
    /// </summary>
    /// <param name="logger">The logging instance.</param>
    /// <param name="channelApi">The channel API.</param>
    /// <param name="mediator">The mediator.</param>
    /// <param name="discordSettings">The Discord settings.</param>
    public MemberApplicationCreatedResponder(
        ILogger<MemberApplicationCreatedResponder> logger,
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
    public async Task<Result> RespondAsync(IMessageCreate ev, CancellationToken ct = default)
    {
        // return if the message has no screenshot;
        if (ev.Attachments.Count == 0
            || ev.Author.IsBot.HasValue && ev.Author.IsBot.Value
            || ev.Author.IsSystem.HasValue && ev.Author.IsSystem.Value
            || !ev.GuildID.HasValue
        )
        {
            return Result.FromSuccess();
        }

        var getChannelNameResult = await _channelApi.GetChannelAsync(ev.ChannelID, ct);
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

        var createMemberAppResult = await _mediator.Send(
            new CreateFromDiscordMessage.Command {DiscordMessageCreatedEvent = ev}, ct);
        if (createMemberAppResult.IsSuccess)
        {
            _logger.LogInformation($"Added a new application for message: {ev.ID}");
        }
        else
        {
            return createMemberAppResult;
        }

        var sendConfirmationResult = await _channelApi.CreateMessageAsync(ev.ChannelID,
            "Your application has been submitted and you will be pinged once it has been processed.",
            messageReference: new MessageReference(ev.ID), ct: ct);
        return sendConfirmationResult.IsSuccess
            ? Result.FromSuccess()
            : Result.FromError(sendConfirmationResult.Error);
    }
}