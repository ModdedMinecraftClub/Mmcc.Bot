using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Services.MessageResponders;

public abstract class MessageResponderBase
{
    private readonly IDiscordRestChannelAPI _channelApi;

    protected MessageResponderBase(IDiscordRestChannelAPI channelApi)
    {
        _channelApi = channelApi;
    }
    
    /// <summary>
    /// Responds to the message the command was invoked by with a message with <see cref="string"/> content. 
    /// </summary>
    /// <param name="message">Content of the response.</param>
    /// <returns>Result of the asynchronous operation.</returns>
    public abstract Task<IResult> Respond(string message);
        
    /// <summary>
    /// Responds to the message the command was invoked by with a message with <see cref="Embed"/>s content.
    /// </summary>
    /// <param name="embeds">Content of the response.</param>
    /// <returns>Result of the asynchronous operation.</returns>
    public async Task<IResult> Respond(params Embed[] embeds) =>
        await Respond(embeds.ToList());
        
    /// <summary>
    /// Responds to the message the command was invoked by with a message with <see cref="Embed"/>s content.
    /// </summary>
    /// <param name="embeds">Content of the response.</param>
    /// <returns>Result of the asynchronous operation.</returns>
    public abstract Task<IResult> Respond(List<Embed> embeds);

    /// <summary>
    /// Responds to the message the command was invoked by with a message with components.
    /// </summary>
    /// <param name="components">Components content of the response.</param>
    /// <param name="content">String content of the response.</param>
    /// <param name="embeds">Embeds content of the response.</param>
    /// <returns></returns>
    public abstract Task<IResult> RespondWithComponents(IReadOnlyList<IMessageComponent> components, Optional<string> content = new(), params Embed[] embeds);
    
    protected async Task<IResult> Respond(Snowflake parentMessageId, Snowflake parentMessageChannelId, string message) =>
        await _channelApi.CreateMessageAsync(
            channelID: parentMessageChannelId,
            content: message,
            messageReference: new MessageReference(parentMessageId, FailIfNotExists: false)
        );
    
    protected async Task<IResult> Respond(Snowflake parentMessageId, Snowflake parentMessageChannelId, List<Embed> embeds) =>
        await _channelApi.CreateMessageAsync(
            channelID: parentMessageChannelId,
            embeds: embeds,
            messageReference: new MessageReference(parentMessageId, FailIfNotExists: false)
        );

    protected async Task<IResult> RespondWithComponents(
        Snowflake parentMessageId,
        Snowflake parentMessageChannelId,
        IReadOnlyList<IMessageComponent> components,
        Optional<string> content = new(),
        params Embed[] embeds
    ) => await _channelApi.CreateMessageAsync(
        channelID: parentMessageChannelId,
        content: content,
        embeds: embeds,
        components: new(components),
        messageReference: new MessageReference(parentMessageId, FailIfNotExists: false)
    );
}