using System;
using System.Collections.Generic;
using System.Linq;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Statics;
using Porbeagle;
using Porbeagle.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Mmcc.Bot.Features.Diagnostics.Views;

[DiscordView]
public partial record DrivesDiagnosticsView : IMessageView
{
    public DrivesDiagnosticsView(IEnumerable<GetDrivesDiagnostics.QueryResult> results)
        => Embed = new DrivesDiagnosticsEmbed(results);
    
    public Optional<string> Text { get; init; } = new();
    public Embed Embed { get; }
}

public record DrivesDiagnosticsEmbed : Embed
{
    public DrivesDiagnosticsEmbed(IEnumerable<GetDrivesDiagnostics.QueryResult> results) : base(
        Title: "Drives diagnostics",
        Colour: ColourPalette.Blue,
        Thumbnail: EmbedProperties.MmccLogoThumbnail,
        Footer: new EmbedFooter("Dedicated server"),
        Timestamp: DateTimeOffset.UtcNow,
        Fields: results.Select(x => x.ToEmbedField()).ToList()
    )
    {
    }
}

file static class DriveDiagnosticsMapperExtensions
{
    internal static IEmbedField ToEmbedField(this GetDrivesDiagnostics.QueryResult d)
    {
        var freeSpaceRemainingEmoji = d.PercentageUsed switch
        {
            <= 65 => ":green_circle:",
            <= 85 => ":yellow_circle:",
            _ => ":red_circle:"
        };

        var fieldTextValue = $"""
        Volume label: {d.Label}
        File system: {d.DriveFormat}
        Available space: {freeSpaceRemainingEmoji} {d.GigabytesFree:0.00} GB ({d.PercentageUsed:0.00}% used)
        Total size: {d.GigabytesTotalSize:0.00} GB
        """;

        return new EmbedField(Name: d.Name, Value: fieldTextValue, IsInline: false);
    }
}
