using System.Collections.Generic;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Services.MessageResponders;

public class InteractionMessageResponder : MessageResponderBase
{
    private readonly InteractionContext _context;

    /// <summary>
    /// Instantiates a new instance of the <see cref="InteractionMessageResponder"/>.
    /// </summary>
    /// <param name="context">The message context.</param>
    /// <param name="channelApi">The channel API.</param>
    public InteractionMessageResponder(InteractionContext context, IDiscordRestChannelAPI channelApi) : base(channelApi)
        => _context = context;

    /// <inheritdoc />
    public override async Task<IResult> Respond(string message) 
        => await Respond(_context.Message.Value.ID, _context.ChannelID, message);

    /// <inheritdoc />
    public override async Task<IResult> Respond(List<Embed> embeds)
        => await Respond(_context.Message.Value.ID, _context.ChannelID, embeds);

    /// <inheritdoc />
    public override async Task<IResult> RespondWithComponents(
        IReadOnlyList<IMessageComponent> components,
        Optional<string> content = new(),
        params Embed[] embeds
    ) => await RespondWithComponents(_context.Message.Value.ID, _context.ChannelID, components, content, embeds);
}