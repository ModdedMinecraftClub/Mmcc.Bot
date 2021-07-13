using System;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Caching.Entities
{
    public record ButtonHandler(
        Func<IInteractionCreate, Task<Result>> Handle,
        Optional<DiscordPermission> RequiredPermission = new()
    );

    public record Button(
        IButtonComponent Component,
        ButtonHandler Handler
    );
}