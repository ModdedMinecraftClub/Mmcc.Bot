using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Statics;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.RemoraAbstractions.Services;

/// <summary>
/// Service for obtaining help embeds.
/// </summary>
public interface IHelpService
{
    /// <summary>
    /// Traverses a command tree and produces help embeds.
    /// </summary>
    /// <param name="nodes">The nodes to traverse.</param>
    /// <param name="embeds">The embeds output.</param>
    void TraverseAndGetHelpEmbeds(IList<IChildNode> nodes, IList<Embed> embeds);
}
    
/// <inheritdoc />
public class HelpService : IHelpService
{
    private readonly IColourPalette _colourPalette;

    /// <summary>
    /// Instantiates a new instance of <see cref="HelpService"/>.
    /// </summary>
    /// <param name="colourPalette">The colour palette.</param>
    public HelpService(IColourPalette colourPalette)
    {
        _colourPalette = colourPalette;
    }

    /// <inheritdoc />
    public void TraverseAndGetHelpEmbeds(IList<IChildNode> nodes, IList<Embed> embeds)
    {
        var orphans = nodes
            .OfType<CommandNode>()
            .ToList();
        var normals = nodes
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
                        $"[`!{g.Key}`{(g.Aliases.Any() ? "/" + string.Join("/", g.Aliases.Select(a => $"`!{a}`")) : "")}]",
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
            TraverseAndGetHelpEmbeds(normal.Children.ToList(), embeds);
        }
    }
}