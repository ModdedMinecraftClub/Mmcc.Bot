using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Statics;
using Mmcc.Bot.RemoraAbstractions.Conditions.Attributes;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Results;

namespace Mmcc.Bot.Commands.Diagnostics;

/// <summary>
/// Diagnostics commands.
/// </summary>
[Group("diagnostics")]
[Description("Server and bot diagnostics")]
public class DiagnosticsCommands : CommandGroup
{
    private readonly IColourPalette _colourPalette;
    private readonly IMediator _mediator;
    private readonly ICommandResponder _responder;

    private readonly Dictionary<string, string> _resourcesToCheck = new()
    {
        ["Discord"] = "discord.com",
        ["Mojang API"] = "api.mojang.com",
        ["MMCC"] = "s4.moddedminecraft.club"
    };

    /// <summary>
    /// Instantiates a new instance of <see cref="DiagnosticsCommands"/>.
    /// </summary>
    /// <param name="colourPalette">The colour palette.</param>
    /// <param name="mediator">The mediator.</param>
    /// <param name="responder">The command responder.</param>
    public DiagnosticsCommands(
        IColourPalette colourPalette,
        IMediator mediator,
        ICommandResponder responder
    )
    {
        _colourPalette = colourPalette;
        _mediator = mediator;
        _responder = responder;
    }

    /// <summary>
    /// Show status of the bot and APIs it uses.
    /// </summary>
    /// <returns>Result of the operation.</returns>
    [Command("bot")]
    [Description("Show status of the bot and APIs it uses")]
    public async Task<IResult> BotDiagnostics()
    {
        var fields = new List<EmbedField>
        {
            new("Bot status", ":green_circle: Operational", false)
        };

        foreach (var (name, address) in _resourcesToCheck)
        {
            var pingResult = await _mediator.Send(new PingNetworkResource.Query {Address = address});
            var fieldVal = !pingResult.IsSuccess || pingResult.Entity.Status != IPStatus.Success
                ? ":x: Could not reach."
                : pingResult.Entity.RoundtripTime switch
                {
                    <= 50 => ":green_circle: ",
                    <= 120 => ":yellow_circle: ",
                    _ => ":red_circle: "
                } + pingResult.Entity.RoundtripTime + " ms";
                
            fields.Add(new($"{name} Status", fieldVal, false));
        }

        var embed = new Embed
        {
            Title = "Bot diagnostics",
            Description = "Information about the status of the bot and the APIs it uses",
            Fields = fields,
            Timestamp = DateTimeOffset.UtcNow,
            Colour = _colourPalette.Green
        };
        return await _responder.Respond(embed);
    }

    /// <summary>
    /// Shows drives info - including free space.
    /// </summary>
    /// <returns>Result of the operation.</returns>
    [Command("drives")]
    [Description("Shows drives info (including free space)")]
    [RequireGuild]
    [RequireUserGuildPermission(DiscordPermission.BanMembers)]
    public async Task<IResult> DrivesDiagnostics()
    {
        var embedFields = new List<EmbedField>();
        var drives = await _mediator.Send(new GetDrives.Query());
            
        foreach (var d in drives)
        {
            var fieldValue = new StringBuilder();
            var spaceEmoji = d.PercentageUsed switch
            {
                <= 65 => ":green_circle:",
                <= 85 => ":yellow_circle:",
                _ => ":red_circle:"
            };
                
            fieldValue.AppendLine($"Volume label: {d.Label}");
            fieldValue.AppendLine($"File system: {d.DriveFormat}");
            fieldValue.AppendLine($"Available space: {spaceEmoji} {d.GigabytesFree:0.00} GB ({d.PercentageUsed:0.00}% used)");
            fieldValue.AppendLine($"Total size: {d.GigabytesTotalSize:0.00} GB");
                
            embedFields.Add(new EmbedField(d.Name, fieldValue.ToString(), false));
        }

        var embed = new Embed
        {
            Title = "Drives diagnostics",
            Colour = _colourPalette.Blue,
            Thumbnail = EmbedProperties.MmccLogoThumbnail,
            Footer = new EmbedFooter("Dedicated server"),
            Timestamp = DateTimeOffset.UtcNow,
            Fields = embedFields
        };
        return await _responder.Respond(embed);
    }
}