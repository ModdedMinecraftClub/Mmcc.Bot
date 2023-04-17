using System.Collections.Generic;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Services.MessageResponders;

public class CommandMessageResponder : MessageResponderBase
{
    private readonly MessageContext _context;

    /// <summary>
    /// Instantiates a new instance of the <see cref="CommandMessageResponder"/>.
    /// </summary>
    /// <param name="context">The message context.</param>
    /// <param name="channelApi">The channel API.</param>
    public CommandMessageResponder(MessageContext context, IDiscordRestChannelAPI channelApi) : base(channelApi)
        => _context = context;

    /// <inheritdoc />
    public override async Task<IResult> Respond(string message)
        => await Respond(_context.MessageID, _context.ChannelID, message);

    /// <inheritdoc />
    public override async Task<IResult> Respond(List<Embed> embeds)
        => await Respond(_context.MessageID, _context.ChannelID, embeds);

    /// <inheritdoc />
    public override async Task<IResult> RespondWithComponents(
        IReadOnlyList<IMessageComponent> components,
        Optional<string> content = new(),
        params Embed[] embeds
    ) => await RespondWithComponents(_context.MessageID, _context.ChannelID, components, content, embeds);
}
