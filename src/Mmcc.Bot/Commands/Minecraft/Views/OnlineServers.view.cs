using System.Collections.Generic;
using Mmcc.Bot.Polychat.Models;
using Porbeagle;
using Porbeagle.Attributes;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Mmcc.Bot.Commands.Minecraft.Views;

[DiscordView]
public sealed partial record OnlineServersView : IMessageView
{
    public OnlineServersView(IEnumerable<OnlineServerInformation> results)
        => Embed = new();
    
    public Optional<string> Text { get; init; } = new();
    
    public Embed Embed { get; }
}

