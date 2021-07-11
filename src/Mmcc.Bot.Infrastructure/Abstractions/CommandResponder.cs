using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Abstractions
{
    /// <summary>
    /// Responds to the message the command was invoked by.
    /// </summary>
    public interface ICommandResponder
    {
        /// <summary>
        /// Responds to the message the command was invoked by with a message with <see cref="string"/> content. 
        /// </summary>
        /// <param name="message">Content of the response.</param>
        /// <returns>Result of the asynchronous operation.</returns>
        Task<IResult> Respond(string message);
        
        /// <summary>
        /// Responds to the message the command was invoked by with a message with <see cref="Embed"/>s content.
        /// </summary>
        /// <param name="embeds">Content of the response.</param>
        /// <returns>Result of the asynchronous operation.</returns>
        Task<IResult> Respond(params Embed[] embeds);
        
        /// <summary>
        /// Responds to the message the command was invoked by with a message with <see cref="Embed"/>s content.
        /// </summary>
        /// <param name="embeds">Content of the response.</param>
        /// <returns>Result of the asynchronous operation.</returns>
        Task<IResult> Respond(List<Embed> embeds);

        /// <summary>
        /// Responds to the message the command was invoked by with a message with components.
        /// </summary>
        /// <param name="components">Components content of the response.</param>
        /// <param name="content">String content of the response.</param>
        /// <param name="embeds">Embeds content of the response.</param>
        /// <returns></returns>
        Task<IResult> RespondWithComponents(IReadOnlyList<IMessageComponent> components, Optional<string> content = new(), params Embed[] embeds);
    }
    
    /// <inheritdoc />
    public class CommandResponder : ICommandResponder
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;

        /// <summary>
        /// Instantiates a new instance of the <see cref="CommandResponder"/>.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        public CommandResponder(MessageContext context, IDiscordRestChannelAPI channelApi)
        {
            _context = context;
            _channelApi = channelApi;
        }

        /// <inheritdoc />
        public async Task<IResult> Respond(string message) =>
            await _channelApi.CreateMessageAsync(
                channelID: _context.ChannelID,
                content: message,
                messageReference: new MessageReference(_context.MessageID, FailIfNotExists: false)
            );

        /// <inheritdoc />
        public async Task<IResult> Respond(params Embed[] embeds) =>
            await Respond(embeds.ToList());

        /// <inheritdoc />
        public async Task<IResult> Respond(List<Embed> embeds) =>
            await _channelApi.CreateMessageAsync(
                channelID: _context.ChannelID,
                embeds: embeds,
                messageReference: new MessageReference(_context.MessageID, FailIfNotExists: false)
            );

        /// <inheritdoc />
        public async Task<IResult> RespondWithComponents(IReadOnlyList<IMessageComponent> components, Optional<string> content = new(), params Embed[] embeds)
        {
            var embedsOptional = !embeds.Any()
                ? new Optional<IReadOnlyList<Embed>>()
                : new Optional<IReadOnlyList<Embed>>(embeds);
            return await _channelApi.CreateMessageAsync(
                channelID: _context.ChannelID,
                content: content,
                embeds: embeds,
                components: new(components),
                messageReference: new MessageReference(_context.MessageID, FailIfNotExists: false)
            );
        }
    }
}