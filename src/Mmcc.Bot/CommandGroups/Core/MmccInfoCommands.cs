using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Mmcc.Bot.Core.Statics;
using Mmcc.Bot.Infrastructure.Abstractions;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Core
{
    public class MmccInfoCommands : CommandGroup
    {
        private readonly ICommandResponder _responder;

        public MmccInfoCommands(ICommandResponder responder)
        {
            _responder = responder;
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
    }
}