using Mmcc.Bot.Common.Statics;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.Common.UI.Buttons;

public record ForumButton() : ButtonComponent(
    ButtonComponentStyle.Link,
    "Forum",
    new PartialEmoji(Name: "🗣️"),
    URL: MmccUrls.Forum
);