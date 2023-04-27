using System;
using System.Collections.Generic;
using System.Linq;
using Mmcc.Bot.Common.Models.Colours;
using Porbeagle;
using Porbeagle.Attributes;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Mmcc.Bot.Features.Guilds;

[DiscordView]
public sealed partial record GetGuildInfoView : IMessageView
{
    public GetGuildInfoView(GetGuildInfo.QueryResult guildInfo)
        => Embed = new GuildInfoEmbed(guildInfo);

    public Optional<string> Text { get; init; } = new();
    
    public Embed Embed { get; }
}

public sealed record GuildInfoEmbed : Embed
{
    public GuildInfoEmbed(GetGuildInfo.QueryResult guildInfo) : base(
        Title: "Guild info",
        Description: "Information about the current guild.",
        Timestamp: DateTimeOffset.UtcNow,
        Colour: ColourPalette.Blue,
        Thumbnail: guildInfo.GuildIconUrl is null 
            ? new Optional<IEmbedThumbnail>() 
            : new EmbedThumbnail(guildInfo.GuildIconUrl.ToString()),
        Fields:new List<EmbedField>
        {
            new(Name: "Name", Value: guildInfo.GuildName, IsInline: false),
            new(Name: "Owner", Value: $"<@{guildInfo.GuildOwnerId}>", IsInline: false),
            new(Name: "Max members", Value: guildInfo.GuildMaxMembers.ToString() ?? "Unavailable", IsInline: false),
            new(Name: "Available roles", Value: guildInfo.GuildRoles.AsDiscordFormattedText(), IsInline: false)
        }
    )
    {
    }
}

file static class RolesListMapperExtensions
{
    internal static string AsDiscordFormattedText(this IEnumerable<IRole> roles)
    {
        var linkableRoles = roles.Select(r => $"<@&{r.ID}>");

        return string.Join(", ", linkableRoles);
    }
}
