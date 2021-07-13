using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Mmcc.Bot.Caching;
using Mmcc.Bot.Caching.Entities;
using Mmcc.Bot.Core.Extensions.Caching;
using Mmcc.Bot.Core.Statics;
using Mmcc.Bot.RemoraAbstractions;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
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
        private readonly IInteractionResponder _interactionResponder;

        public MmccInfoCommands(ICommandResponder responder, IDiscordRestInteractionAPI interactionApi, IDiscordRestChannelAPI channelApi, IButtonHandlerRepository handlerRepository, IDiscordRestWebhookAPI webhookApi, IInteractionResponder interactionResponder)
        {
            _responder = responder;
            _interactionApi = interactionApi;
            _channelApi = channelApi;
            _handlerRepository = handlerRepository;
            _webhookApi = webhookApi;
            _interactionResponder = interactionResponder;
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
            var testButton = new ButtonBuilder(ButtonComponentStyle.Primary)
                .WithLabel("Test")
                .WithHandler(async ev =>
                    await _interactionResponder.RespondAsynchronously(
                        ev.ID, ev.Token,
                        () => ValueTask.FromResult<Result<IEnumerable<Embed>>>(new Embed[] { new("Title") })))
                .Build()
                .RegisterWith(_handlerRepository);
            return await _responder.RespondWithComponents(DiscordUI.FromButtons(testButton), "Test buttons");
        }
    }
}