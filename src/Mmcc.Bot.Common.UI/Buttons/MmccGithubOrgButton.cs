using Mmcc.Bot.Common.Statics;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Mmcc.Bot.Common.UI.Buttons;

public record MmccGithubOrgButton() : ButtonComponent(
    ButtonComponentStyle.Link, 
    "GitHub",
    new PartialEmoji(new Snowflake(453413238638641163)),
    URL: MmccUrls.GitHub
);