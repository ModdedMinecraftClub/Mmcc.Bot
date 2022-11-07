using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mmcc.Bot.Common.Extensions.System;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Models.Settings;
using Mmcc.Bot.Common.Statics;
using Mmcc.Bot.RemoraAbstractions.Conditions.CommandSpecific;
using Remora.Commands.Trees.Nodes;
using Remora.Discord.API.Objects;
using Remora.Discord.Extensions.Formatting;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Services;

public interface IHelpService
{
    Embed GetHelpForAll();
    Result<Embed> GetHelpForCategory(List<string> pathToCategory);
}

public class HelpService : IHelpService
{
    private const string CategoryIcon = ":file_folder:";
    private const string CommandIcon = "❯";
    
    private readonly IColourPalette _colourPalette;
    private readonly DiscordSettings _discordSettings;
    private readonly CommandTreeWalker _cmdTreeWalker;

    public HelpService(
        IColourPalette colourPalette,
        CommandTreeWalker cmdTreeWalker,
        DiscordSettings discordSettings
    )
    {
        _colourPalette = colourPalette;
        _cmdTreeWalker = cmdTreeWalker;
        _discordSettings = discordSettings;
    }

    public Embed GetHelpForAll()
    {
        var categoryEmbedFields = new List<EmbedField>();
        _cmdTreeWalker.PreOrderTraverseParentNodes(node =>
        {
            if (node is not GroupNode groupNode)
                return;

            var embedFieldForCategory = GetEmbedFieldForCategory(groupNode);
            categoryEmbedFields.Add(embedFieldForCategory);
        });

        var helpEmbed = new Embed
        {
            Title = ":information_source: Help",
            Description = "Shows available categories. To see commands for a given category use `!help <categoryName>`.",
            Fields = categoryEmbedFields,
            Colour = _colourPalette.Blue,
            Thumbnail = EmbedProperties.MmccLogoThumbnail
        };

        return helpEmbed;
    }

    public Result<Embed> GetHelpForCategory(List<string> pathToCategory)
    {
        var category = _cmdTreeWalker.GetGroupNodeByPath(pathToCategory);
        if (category is null)
            return Result<Embed>.FromError(
                new NotFoundError($"No category matches {Markdown.InlineCode(string.Join(" ", pathToCategory))}")
            );

        var formattedPath = GetFormattedPathForCategory(category);
        
        var embedTitle = $"{CategoryIcon} {category.Description} [{formattedPath}]";
        var embedDescription = $"Usage: {formattedPath[..^1]} <command name> <params>`";
        var embedFields = GetCommandsEmbedFieldsForCategory(category);

        var embed = new Embed
        {
            Title = embedTitle,
            Description = embedDescription,
            Fields = embedFields,
            Colour = _colourPalette.Blue,
            Thumbnail = EmbedProperties.MmccLogoThumbnail
        };
        
        return embed;
    }

    private List<EmbedField> GetCommandsEmbedFieldsForCategory(GroupNode category)
        => category.Children
            .OfType<CommandNode>()
            .Select(GetEmbedFieldForCommand)
            .ToList();

    private EmbedField GetEmbedFieldForCommand(CommandNode cmd)
    {
        var cmdDescription = cmd.Shape.Description;
        var cmdArgs = cmd.Shape.Parameters;
        var cmdArgsFormatted = string.Join(" ", cmdArgs.Select(x => $"<{x.HintName}>"));
        
        var fieldName = $"{CommandIcon} {cmd.Key} {cmdArgsFormatted}";
        
        var fieldDescAliasesLine = cmd.Aliases.Any()
            ? $"{Markdown.Underline("Aliases:")} {string.Join(", ", cmd.Aliases.Select(x => x.DoubleQuotes()))}\n"
            : "";
        
        var requiredPermission = cmd.CommandMethod.GetCustomAttribute(typeof(RequireUserGuildPermissionAttribute));
        var requiredPermissionLine = requiredPermission is RequireUserGuildPermissionAttribute r
            ? $"{Markdown.Underline("Required user permission:")} {r.Permission}\n"
            : "";
        
        var fieldDescription = $"{fieldDescAliasesLine}{requiredPermissionLine}{cmdDescription}";

        return new EmbedField(fieldName, fieldDescription, false);
    }

    private EmbedField GetEmbedFieldForCategory(GroupNode category)
    {
        var formattedPath = GetFormattedPathForCategory(category);
        var fullHelpCmd = $"!help {formattedPath}";
        
        var fieldName = $"{CategoryIcon} {category.Description} [{formattedPath}]";
        var fieldDesc = $"Full help: {Markdown.InlineCode(fullHelpCmd)}";

        return new EmbedField(fieldName, fieldDesc, false);
    }

    private string GetFormattedPathForCategory(GroupNode category)
    {
        var prefix = _discordSettings.Prefix;
        var path = category.Parent is GroupNode
            ? string.Join(" ", _cmdTreeWalker.CollectPath(category))
            : category.Key;
        var formattedPath = Markdown.InlineCode($"{prefix}{path}");

        return formattedPath;
    }
}