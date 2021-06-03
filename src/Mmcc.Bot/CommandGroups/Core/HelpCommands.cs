using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Statics;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Commands.Trees;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Core
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
        private readonly IDiscordRestUserAPI _userApi;

        /// <summary>
        /// Instantiates a new instance of <see cref="HelpCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="tree">The command tree.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="userApi">The user API.</param>
        public HelpCommands(MessageContext context, IDiscordRestChannelAPI channelApi, CommandTree tree, ColourPalette colourPalette, IDiscordRestUserAPI userApi)
        {
            _context = context;
            _channelApi = channelApi;
            _tree = tree;
            _colourPalette = colourPalette;
            _userApi = userApi;
        }

        [Command("help")]
        [Description("Shows available commands")]
        public async Task<IResult> Help()
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
            
            Traverse(_tree.Root.Children.ToList(), embeds);

            // this might create problems in the future, I'm not sure;
            var createDmResult = await _userApi.CreateDMAsync(_context.User.ID);
            if (!createDmResult.IsSuccess)
            {
                return createDmResult;
            }

            foreach (var embed in embeds)
            {
                var sendEmbedResult = await _channelApi.CreateMessageAsync(createDmResult.Entity.ID, embed: embed);
                if (!sendEmbedResult.IsSuccess)
                {
                    return sendEmbedResult;
                }
            }

            return await _channelApi.CreateMessageAsync(_context.ChannelID, "Help has been sent to your DMs :smile:.",
                messageReference: new MessageReference(_context.MessageID, FailIfNotExists: true));
        }
        
        /// <summary>
        /// Traverses the command tree and produces a field for each <see cref="CommandGroup"/>.
        /// </summary>
        /// <param name="children">Children of the root node.</param>
        /// <param name="embeds">List of embeds.</param>
        private void Traverse(IList<IChildNode> children, IList<Embed> embeds)
        {
            var orphans = children
                .OfType<CommandNode>()
                .ToList();
            var normals = children
                .OfType<GroupNode>()
                .ToList();
            var fields = new List<EmbedField>();
            
            foreach (var orphan in orphans)
            {
                var nameString = new StringBuilder();
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

                    nameString.AppendLine($"❯ {orphan.Key} {paramsString}");
                }
                else
                {
                    nameString.AppendLine($"❯ {orphan.Key}");
                }

                var fieldValueSb = new StringBuilder();
                if (orphan.Aliases.Any())
                {
                    fieldValueSb.Append("**Aliases:** ");
                    for (var i = 0; i < orphan.Aliases.Count; i++)
                    {
                        fieldValueSb.Append(i != orphan.Aliases.Count - 1
                            ? $"\"{orphan.Aliases[i]}\", "
                            : $"\"{orphan.Aliases[i]}\"");
                    }
                }
                fieldValueSb.Append("\n" + orphan.Shape.Description);
                
                fields.Add(new EmbedField(nameString.ToString(), $"{fieldValueSb}", false));
            }
            
            var parent = orphans.FirstOrDefault()?.Parent;
            var embed = parent switch
            {
                GroupNode g => new Embed
                {
                    // what the fuck??
                    Title = $":arrow_right: {g.Description} " +
                            $"[`!{g.Key}`{(g.Aliases.Any() ? " / " + string.Join("/", g.Aliases.Select(a => $"`!{a}`")) : "")}]",
                    Description = $"Usage: `!{g.Key} <command name> <params>`."
                },
                _ => new Embed
                {
                    Title = ":arrow_right: General commands [`!`]",
                    Description = "Usage: `!<command name> <params>`."
                }
            };
            embed = embed with
            {
                Fields = fields,
                Colour = _colourPalette.Blue,
                Thumbnail = EmbedProperties.MmccLogoThumbnail
            };
            
            embeds.Add(embed);

            foreach (var normal in normals)
            {
                Traverse(normal.Children.ToList(), embeds);
            }
        }
    }
}