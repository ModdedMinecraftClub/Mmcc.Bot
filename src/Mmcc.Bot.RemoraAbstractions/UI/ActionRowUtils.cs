using System.Collections.Generic;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.RemoraAbstractions.UI;

public static class ActionRowUtils
{
    public static ActionRowComponent CreateActionRowWithComponents(params IMessageComponent[] childComponents)
        => new(childComponents);
}