using Mmcc.Bot.Common.Extensions.System;
using Mmcc.Bot.Common.Models;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Mmcc.Bot.Common.UI.Embeds;

public record NotificationEmbed : Embed
{
    public NotificationEmbed(Notification context) : base(
        Title: context.Title,
        Description: context.Description ?? new Optional<string>(),
        Timestamp: context.Timestamp ?? new Optional<DateTimeOffset>(),
        Fields: context.CustomProperties?.ToEmbedFields().ToList() ?? new Optional<IReadOnlyList<IEmbedField>>()
    )
    {
    }
}