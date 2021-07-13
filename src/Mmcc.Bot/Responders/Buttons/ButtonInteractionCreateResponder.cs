using System;
using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.Caching;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Infrastructure.Services;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Mmcc.Bot.Responders.Buttons
{
    public class ButtonInteractionCreateResponder : IResponder<IInteractionCreate>
    {
        private readonly IButtonHandlerRepository _handlerRepository;
        private readonly IDiscordPermissionsService _permissionsService;
        private readonly IDiscordRestInteractionAPI _interactionApi;

        public ButtonInteractionCreateResponder(
            IButtonHandlerRepository handlerRepository,
            IDiscordPermissionsService permissionsService,
            IDiscordRestInteractionAPI interactionApi
        )
        {
            _handlerRepository = handlerRepository;
            _permissionsService = permissionsService;
            _interactionApi = interactionApi;
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
            var handler = _handlerRepository.GetOrDefault(new Guid(customId));

            if (handler is null)
            {
                return Result.FromError(
                    new NotFoundError($"Could not find a button handler for button with ID: {customId}."));
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