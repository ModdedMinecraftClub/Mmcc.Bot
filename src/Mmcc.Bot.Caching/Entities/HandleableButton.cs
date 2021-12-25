using MediatR;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Caching.Entities;

public record HandleableButton
{
    public Snowflake Id { get; init; }
    public IButtonComponent Component { get; init; }
    public ButtonHandler Handler { get; init; }
    
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
        where THandlerCommand : IRequest<Result>
        where TContext : class, new()
        => new(id, component, new(typeof(THandlerCommand), typeof(TContext), context, requiredPermission));

    public void Deconstruct(out Snowflake id, out IButtonComponent component, out ButtonHandler handler)
    {
        id = Id;
        component = Component;
        handler = Handler;
    }
}