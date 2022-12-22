using Mmcc.Bot.Common.UI.Buttons;
using Porbeagle;
using Porbeagle.Attributes;
using Remora.Rest.Core;

namespace Mmcc.Bot.Commands.MmccInfo;

[DiscordView]
public partial record MmccInfoView : IMessageView
{
    public Optional<string> Text { get; init; } = "Useful links";

    [ActionRow(0)]
    private MmccWebsiteButton MmccWebsiteButton { get; } = new();

    [ActionRow(0)]
    private DonateButton DonateButton { get; } = new();

    [ActionRow(0)]
    private WikiButton WikiButton { get; } = new();

    [ActionRow(0)]
    private ForumButton ForumButton { get; } = new();

    [ActionRow(0)]
    private MmccGithubOrgButton MmccGitHubOrgButton { get; } = new();
}