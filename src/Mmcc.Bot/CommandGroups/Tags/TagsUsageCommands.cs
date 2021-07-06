using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Infrastructure.Queries.Tags;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Tags
{
    /// <summary>
    /// Commands that allow the usage of tags.
    /// </summary>
    public class TagsUsageCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IMediator _mediator;
        private readonly IDiscordRestChannelAPI _channelApi;

        /// <summary>
        /// Instantiates a new instance of <see cref="TagsUsageCommands"/>.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="channelApi">The channel API.</param>
        public TagsUsageCommands(MessageContext context, IMediator mediator, IDiscordRestChannelAPI channelApi)
        {
            _context = context;
            _mediator = mediator;
            _channelApi = channelApi;
        }

        [Command("tag", "t")]
        [Description("Sends a given tag.")]
        public async Task<IResult> SendTag(string tagName)
        {
            return await _mediator.Send(new GetOne.Query(_context.GuildID.Value, tagName)) switch
            {
                {IsSuccess: true, Entity: { } e} =>
                    await _channelApi.CreateMessageAsync(_context.ChannelID, e.Content),

                {IsSuccess: true} =>
                    Result.FromError(
                        new NotFoundError($"Could not find a tag with name {tagName} belonging to the current guild.")),
                
                {IsSuccess: false} res => res
            };
        }
    }
}