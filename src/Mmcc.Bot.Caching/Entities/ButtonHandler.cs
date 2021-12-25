using System;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Core;

namespace Mmcc.Bot.Caching.Entities;

public record ButtonHandler(
    Type HandlerCommandType,
    Type ContextType,
    object Context,
    Optional<DiscordPermission> RequiredPermission = new()
);