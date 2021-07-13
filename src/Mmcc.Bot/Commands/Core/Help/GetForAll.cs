using System.Collections.Generic;
using System.Linq;
using MediatR;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Statics;
using Remora.Commands.Trees;
using Remora.Discord.API.Objects;
using Remora.Results;

namespace Mmcc.Bot.Commands.Core.Help
{
    /// <summary>
    /// Gets help for all available commands, maintaining the grouped structure.
    /// </summary>
    public class GetForAll
    {
        /// <summary>
        /// Query to get all available commands, maintaining the grouped structure.
        /// </summary>
        public record Query : IRequest<Result<IList<Embed>>>;
        
        public class Handler : RequestHandler<Query, Result<IList<Embed>>>
        {
            private readonly CommandTree _commandTree;
            private readonly IColourPalette _colourPalette;
            private readonly IHelpService _helpService;

            public Handler(CommandTree commandTree, IColourPalette colourPalette, IHelpService helpService)
            {
                _commandTree = commandTree;
                _colourPalette = colourPalette;
                _helpService = helpService;
            }

            protected override Result<IList<Embed>> Handle(Query request)
            {
                var embeds = new List<Embed>
                {
                    new()
                    {
                        Title = ":information_source: Help",
                        Description = "Shows available commands by category",
                        Colour = _colourPalette.Blue,
                        Thumbnail = EmbedProperties.MmccLogoThumbnail
                    }
                };

                _helpService.TraverseAndGetHelpEmbeds(_commandTree.Root.Children.ToList(), embeds);

                return embeds;
            }
        }
    }
}