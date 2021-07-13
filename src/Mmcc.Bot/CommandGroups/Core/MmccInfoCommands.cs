using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using Mmcc.Bot.Caching;
using Mmcc.Bot.Caching.Entities;
using Mmcc.Bot.Core.Statics;
using Mmcc.Bot.Infrastructure.Abstractions;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Core
{
    public class MmccInfoCommands : CommandGroup
    {
        private readonly ICommandResponder _responder;
        private readonly IDiscordRestInteractionAPI _interactionApi;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestWebhookAPI _webhookApi;
        private readonly IButtonHandlerRepository _handlerRepository;

        public MmccInfoCommands(ICommandResponder responder, IDiscordRestInteractionAPI interactionApi, IDiscordRestChannelAPI channelApi, IButtonHandlerRepository handlerRepository, IDiscordRestWebhookAPI webhookApi)
        {
            _responder = responder;
            _interactionApi = interactionApi;
            _channelApi = channelApi;
            _handlerRepository = handlerRepository;
            _webhookApi = webhookApi;
        }

        [Command("mmcc")]
        [Description("Shows useful MMCC links")]
        public async Task<IResult> Mmcc()
        {
            var usefulLinks = new List<IMessageComponent>
            {
                new ActionRowComponent(new List<ButtonComponent>
                {
                    new(ButtonComponentStyle.Link, "Website", new PartialEmoji(new Snowflake(863798570602856469)), URL: MmccUrls.Website),
                    new(ButtonComponentStyle.Link, "Donate", new PartialEmoji(Name: "❤️"), URL: MmccUrls.Donations),
                    new(ButtonComponentStyle.Link, "Wiki", new PartialEmoji(Name: "📖"), URL: MmccUrls.Wiki),
                    new(ButtonComponentStyle.Link, "Forum", new PartialEmoji(Name: "🗣️"), URL: MmccUrls.Forum),
                    new(ButtonComponentStyle.Link, "GitHub", new PartialEmoji(new Snowflake(453413238638641163)), URL: MmccUrls.GitHub)
                })
            };

            return await _responder.RespondWithComponents(usefulLinks, "Useful links");
        }

        [Command("test")]
        public async Task<IResult> Test()
        {
            var testButtonGuid = new Guid();
            var testButton = new Button(
                new ButtonComponent(ButtonComponentStyle.Primary, "Test", CustomID: testButtonGuid.ToString()),
                new ButtonHandler(async ev =>
                {
                    await _interactionApi.CreateInteractionResponseAsync(ev.ID, ev.Token,
                        new InteractionResponse(InteractionCallbackType.DeferredUpdateMessage));

                    var res = await _webhookApi.CreateFollowupMessageAsync(new(618231038744854539), ev.Token, "responseee");

                    return res.IsSuccess ? Result.FromSuccess() : Result.FromError(res);
                })
            );
            var buttons = new List<IMessageComponent>
            {
                new ActionRowComponent(new List<IButtonComponent>
                {
                    testButton.Component
                })
            };

            _handlerRepository.Register(testButtonGuid, testButton.Handler);
            return await _responder.RespondWithComponents(buttons, "Test buttons");
        }
    }
}