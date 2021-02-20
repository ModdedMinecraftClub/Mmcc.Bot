using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Mmcc.Bot.Core.Models;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups
{
    /// <summary>
    /// Help commands.
    /// </summary>
    public class HelpCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly CommandTree _tree;
        private readonly ColourPalette _colourPalette;
        
        /// <summary>
        /// Instantiates a new instance of <see cref="HelpCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="tree">The command tree.</param>
        /// <param name="colourPalette">The colour palette.</param>
        public HelpCommands(MessageContext context, IDiscordRestChannelAPI channelApi, CommandTree tree, ColourPalette colourPalette)
        {
            _context = context;
            _channelApi = channelApi;
            _tree = tree;
            _colourPalette = colourPalette;
        }

        [Command("help")]
        [Description("Shows available commands")]
        public async Task<IResult> Help()
        {
            var fields = new List<EmbedField>();
            Traverse(_tree.Root.Children.ToList(), ref fields);
            var embed = new Embed
            {
                Title = ":information_source: Help",
                Description = "Shows available commands",
                Fields = fields,
                Colour = _colourPalette.Blue
            };

            var sendEmbedResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendEmbedResult.IsSuccess
                ? Result.FromError(sendEmbedResult)
                : Result.FromSuccess();
        }
        
        /// <summary>
        /// Traverses the command tree and produces a field for each <see cref="CommandGroup"/>.
        /// </summary>
        /// <param name="children">Children of the root node.</param>
        /// <param name="fields">List of fields.</param>
        private static void Traverse(IList<IChildNode> children, ref List<EmbedField> fields)
        {
            var orphans = children
                .OfType<CommandNode>()
                .ToList();
            var normals = children
                .OfType<GroupNode>()
                .ToList();
            
            var valueStr = new StringBuilder();
            foreach (var orphan in orphans)
            {
                if (orphan is null) break;

                var orphanParams = orphan.Shape.Parameters;

                if (orphanParams.Any())
                {
                    var paramsString = new StringBuilder();

                    for (var i = 0; i < orphanParams.Count; i++)
                    {
                        paramsString.Append(i != orphanParams.Count - 1
                            ? $"<{orphanParams[i].HintName}> "
                            : $"<{orphanParams[i].HintName}>");
                    }

                    valueStr.AppendLine($"▸ {orphan.Key} {paramsString}");
                }
                else
                {
                    valueStr.AppendLine($"▸ {orphan.Key}");
                }
                
                valueStr.AppendLine($"*{orphan.Shape.Description}*");
                valueStr.AppendLine();
            }
            
            var parent = orphans.FirstOrDefault()?.Parent;
            var field = parent switch
            {
                GroupNode g => new EmbedField($"▼ {g.Description} (!{g.Key})", valueStr.ToString(), false),
                _ => new EmbedField("▼ General commands", valueStr.ToString(), false)
            };
            fields.Add(field);

            foreach (var normal in normals)
            {
                Traverse(normal.Children.ToList(), ref fields);
            }
        }
    }
}