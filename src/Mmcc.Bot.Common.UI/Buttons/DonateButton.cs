using Mmcc.Bot.Common.Statics;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.Common.UI.Buttons;

public record DonateButton() : ButtonComponent(
    ButtonComponentStyle.Link,
    "Donate",
    new PartialEmoji(Name: "❤️"),
    URL: MmccUrls.Donations
);