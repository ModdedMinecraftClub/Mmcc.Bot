using Mmcc.Bot.Common.Statics;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.Common.UI.Buttons;

public record DonateButton() : ButtonComponent(
    Style: ButtonComponentStyle.Link,
    Label: "Donate",
    Emoji: new PartialEmoji(Name: "❤️"),
    URL: MmccUrls.Donations
);