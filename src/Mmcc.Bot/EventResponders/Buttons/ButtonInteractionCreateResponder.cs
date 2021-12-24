using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Caching;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Statics;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Mmcc.Bot.EventResponders.Buttons;

public class ButtonInteractionCreateResponder : IResponder<IInteractionCreate>
{
    private readonly IButtonHandlerRepository _handlerRepository;
    private readonly IDiscordPermissionsService _permissionsService;
    private readonly IColourPalette _colourPalette;
    private readonly ILogger<ButtonInteractionCreateResponder> _logger;
    private readonly IInteractionResponder _interactionResponder;
    private readonly IMediator _mediator;

    public ButtonInteractionCreateResponder(
        IButtonHandlerRepository handlerRepository,
        IDiscordPermissionsService permissionsService,
        IColourPalette colourPalette,
        ILogger<ButtonInteractionCreateResponder> logger,
        IInteractionResponder interactionResponder,
        IMediator mediator
    )
    {
        _handlerRepository = handlerRepository;
        _permissionsService = permissionsService;
        _colourPalette = colourPalette;
        _logger = logger;
        _interactionResponder = interactionResponder;
        _mediator = mediator;
    }

    public async Task<Result> RespondAsync(IInteractionCreate ev, CancellationToken ct = new())
    {
        if (ev.Type is not InteractionType.MessageComponent
            || !ev.Message.HasValue
            || !ev.Member.HasValue
            || !ev.Member.Value.User.HasValue
            || !ev.ChannelID.HasValue
            || !ev.Data.HasValue
            || !ev.Data.Value.CustomID.HasValue
        )
        {
            return Result.FromSuccess();
        }

        var notifyAboutDeferredRes = await _interactionResponder.NotifyDeferredMessageIsComing(ev.ID, ev.Token, ct);
        if (!notifyAboutDeferredRes.IsSuccess)
        {
            return notifyAboutDeferredRes;
        }
            
        var customId = ev.Data.Value.CustomID.Value;
        var idParseSuccessful = Snowflake.TryParse(customId, out var id);
        if (!idParseSuccessful)
        {
            var errorEmbed = new Embed
            {
                Title = ":x: Invalid custom ID format",
                Description =
                    $"The custom ID: {customId} was not recognised as a valid Mmcc.Bot button ID.",
                Thumbnail = EmbedProperties.MmccLogoThumbnail,
                Colour = _colourPalette.Red,
                Timestamp = DateTimeOffset.UtcNow
            };
            var errSendRes = await _interactionResponder.SendFollowup(ev.Token, errorEmbed);
            return errSendRes.IsSuccess
                ? Result.FromError(new ParsingError<Snowflake>(customId, "Could not parse button ID."))
                : errSendRes;
        }

        var ulongId = id!.Value.Value;
        var handler = _handlerRepository.GetOrDefault(ulongId);
        if (handler is null)
        {
            _logger.LogWarning("Could not find a button handler for button with ID: {customId}.", ulongId);
                
            var errorEmbed = new Embed
            {
                Title = ":x: Interaction expired",
                Description =
                    "This button has expired. Request the command with the button again to generate a new button.",
                Thumbnail = EmbedProperties.MmccLogoThumbnail,
                Colour = _colourPalette.Red,
                Timestamp = DateTimeOffset.UtcNow
            };
            return await _interactionResponder.SendFollowup(ev.Token, errorEmbed);
        }

        // check if user has permission;
        // ReSharper disable once InvertIf
        if (handler.RequiredPermission.HasValue)
        {
            var checkResult = await _permissionsService.CheckHasRequiredPermission(handler.RequiredPermission.Value,
                ev.ChannelID.Value, ev.Member.Value.User.Value, ct);

            // ReSharper disable once InvertIf
            if (!checkResult.IsSuccess)
            {
                var errorEmbed = new Embed
                {
                    Title = ":x: Unauthorised",
                    Description =
                        "You do not have permission to use this button.",
                    Fields = new List<EmbedField>
                    {
                        new("Required permission", $"`{handler.RequiredPermission.Value.ToString()}`")
                    },
                    Thumbnail = EmbedProperties.MmccLogoThumbnail,
                    Colour = _colourPalette.Red,
                    Timestamp = DateTimeOffset.UtcNow
                };
                return await _interactionResponder.SendFollowup(ev.Token, errorEmbed);
            }
        }

        var context = handler.Context;
        var command = Activator.CreateInstance(handler.HandlerCommandType, ev.Token, context);

        return (Result) (await _mediator.Send(command!, ct))!;
    }
}