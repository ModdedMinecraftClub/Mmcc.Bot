using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Mmcc.Bot.Common.Models.Colours;
using Porbeagle;
using Porbeagle.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Mmcc.Bot.Features.Diagnostics;

[DiscordView]
public sealed partial record GetBotDiagnosticsView : IMessageView
{
    public GetBotDiagnosticsView(IEnumerable<GetBotDiagnostics.QueryResult> results)
        => Embed = new BotDiagnosticsEmbed(results);

    public Optional<string> Text { get; init; } = new();
    
    public Embed Embed { get; }
}

public sealed record BotDiagnosticsEmbed : Embed
{
    public BotDiagnosticsEmbed(IEnumerable<GetBotDiagnostics.QueryResult> results) : base(
        Title: "Bot diagnostics",
        Description: "Information about the status of the bot and the APIs it uses",
        Timestamp: DateTimeOffset.UtcNow,
        Colour: ColourPalette.Green,
        Fields: GetFields(results)
    )
    {
    }

    private static Optional<IReadOnlyList<IEmbedField>> GetFields(IEnumerable<GetBotDiagnostics.QueryResult> results)
    {
        var fields = new List<IEmbedField>
        {
            new EmbedField(Name: "Bot status", Value: ":green_circle: Operational", IsInline: false)
        };
        
        fields.AddRange(results.Select(x => x.ToEmbedField()));

        return fields;
    }
}

file static class PingResultMapperExtensions
{
    internal static IEmbedField ToEmbedField(this GetBotDiagnostics.QueryResult pingResult)
    {
        var fieldTextValue = pingResult.Status switch
        {
            IPStatus.Success => pingResult.RoundtripTime switch
            {
                <= 50 => ":green_circle: ",
                <= 120 => ":yellow_circle: ",
                _ => ":red_circle: "
            } + pingResult.RoundtripTime + " ms",

            _ => ":x: Could not reach."
        };

        return new EmbedField(Name: $"{pingResult.Name} Status", Value: fieldTextValue, IsInline: false);
    }
}