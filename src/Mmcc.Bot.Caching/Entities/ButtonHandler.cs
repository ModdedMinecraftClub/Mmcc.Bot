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

public record HandleableButton
{
    private HandleableButton(Snowflake id, IButtonComponent component, ButtonHandler handler)
    {
        Id = id;
        Component = component;
        Handler = handler;
    }

    public static HandleableButton Create<THandlerCommand, TContext>(
        Snowflake id,
        IButtonComponent component,
        TContext context,
        Optional<DiscordPermission> requiredPermission = new()
    )
    {
        return new(id, component, new(typeof(THandlerCommand), typeof(TContext), context!, requiredPermission));
    }

    public Snowflake Id { get; init; }
    public IButtonComponent Component { get; init; }
    public ButtonHandler Handler { get; init; }

    public void Deconstruct(out Snowflake id, out IButtonComponent component, out ButtonHandler handler)
    {
        id = Id;
        component = Component;
        handler = Handler;
    }
};