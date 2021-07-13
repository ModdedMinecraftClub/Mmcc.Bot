using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Caching;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Statics;
using Mmcc.Bot.Infrastructure.Services;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Mmcc.Bot.Responders.Buttons
{
    public class ButtonInteractionCreateResponder : IResponder<IInteractionCreate>
    {
        private readonly IButtonHandlerRepository _handlerRepository;
        private readonly IDiscordPermissionsService _permissionsService;
        private readonly IDiscordRestInteractionAPI _interactionApi;
        private readonly IColourPalette _colourPalette;
        private readonly ILogger<ButtonInteractionCreateResponder> _logger;

        public ButtonInteractionCreateResponder(
            IButtonHandlerRepository handlerRepository,
            IDiscordPermissionsService permissionsService,
            IDiscordRestInteractionAPI interactionApi,
            IColourPalette colourPalette,
            ILogger<ButtonInteractionCreateResponder> logger
        )
        {
            _handlerRepository = handlerRepository;
            _permissionsService = permissionsService;
            _interactionApi = interactionApi;
            _colourPalette = colourPalette;
            _logger = logger;
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
            //var associatedMessage = ev.Message.Value.ID;
            //var user = ev.Member.Value.User.Value.ID;
            //var interactionId = ev.ID;
            
            var customId = ev.Data.Value.CustomID.Value;
            var idParseSuccessful = Snowflake.TryParse(customId, out var id);
            if (!idParseSuccessful)
            {
                return Result.FromError(new ParsingError<Snowflake>(customId, "Could not parse button ID."));
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
                var response = new InteractionResponse(
                    InteractionCallbackType.ChannelMessageWithSource,
                    new InteractionApplicationCommandCallbackData(Embeds: new List<Embed> { errorEmbed })
                );
                var errSendRes = await _interactionApi.CreateInteractionResponseAsync(ev.ID, ev.Token, response, ct);
                return !errSendRes.IsSuccess
                    ? errSendRes
                    : Result.FromSuccess();
            }
            
            var createInteractionRes = await _interactionApi.CreateInteractionResponseAsync(
                ev.ID,
                ev.Token,
                new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage),
                ct
            );
            if (!createInteractionRes.IsSuccess)
            {
                return createInteractionRes;
            }
            
            // check if user has permission;
            // ReSharper disable once InvertIf
            if (handler.RequiredPermission.HasValue)
            {
                var checkResult = await _permissionsService.CheckHasRequiredPermission(handler.RequiredPermission.Value,
                    ev.ChannelID.Value, ev.Member.Value.User.Value, ct);

                if (!checkResult.IsSuccess)
                {
                    return checkResult;
                }
            }

            return await handler.Handle(ev);
        }
    }
}