using Mmcc.Bot.Common.Statics;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Mmcc.Bot.Common.UI.Buttons;

public record MmccWebsiteButton() : ButtonComponent(
        ButtonComponentStyle.Link,
        "Website",
        new PartialEmoji(new Snowflake(863798570602856469)),
        URL: MmccUrls.Website
);