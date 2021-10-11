﻿using System.Collections.Generic;
using System.Linq;
using Mmcc.Bot.Caching.Entities;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.RemoraAbstractions.Ui;

// ReSharper disable once InconsistentNaming
public static class ActionRowUtils
{
    public static List<IMessageComponent> FromButtons(params Button[] buttons) =>
        new()
        {
            new ActionRowComponent(buttons.Select(b => b.Component).ToList())
        };
}