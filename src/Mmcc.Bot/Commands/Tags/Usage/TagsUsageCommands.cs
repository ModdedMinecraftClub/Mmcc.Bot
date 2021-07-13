using System.ComponentModel;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Common.Errors;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.Commands.Tags.Usage
{
    /// <summary>
    /// Commands that allow the usage of tags.
    /// </summary>
    public class TagsUsageCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IMediator _mediator;
        private readonly ICommandResponder _responder;

        /// <summary>
        /// Instantiates a new instance of <see cref="TagsUsageCommands"/>.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="responder">The command responder.</param>
        public TagsUsageCommands(
            MessageContext context,
            IMediator mediator,
            ICommandResponder responder
        )
        {
            _context = context;
            _mediator = mediator;
            _responder = responder;
        }

        [Command("tag", "t")]
        [Description("Sends a given tag.")]
        public async Task<IResult> SendTag(string tagName)
        {
            return await _mediator.Send(new GetOne.Query(_context.GuildID.Value, tagName)) switch
            {
                {IsSuccess: true, Entity: { } e} =>
                    await _responder.Respond(e.Content),

                {IsSuccess: true} =>
                    Result.FromError(
                        new NotFoundError($"Could not find a tag with name {tagName} belonging to the current guild.")),
                
                {IsSuccess: false} res => res
            };
        }
    }
}