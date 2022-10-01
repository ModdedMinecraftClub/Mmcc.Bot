using System.Collections.Generic;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.RemoraAbstractions.UI.Extensions;

public static class MessageComponentExtensions
{
    public static List<IMessageComponent> AsList(this IMessageComponent c) => new() {c};
}