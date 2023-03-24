using Mmcc.Bot.Common.Statics;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.Common.UI.Buttons;

public record WikiButton() : ButtonComponent(
    ButtonComponentStyle.Link,
    "Wiki",
    new PartialEmoji(Name: "📖"),
    URL: MmccUrls.Wiki
);