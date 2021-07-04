using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Infrastructure.Abstractions;
using Mmcc.Bot.Infrastructure.Queries.Help;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Core
{
    /// <summary>
    /// Help commands.
    /// </summary>
    public class HelpCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestUserAPI _userApi;
        private readonly ICommandResponder _responder;
        private readonly IMediator _mediator;

        /// <summary>
        /// Instantiates a new instance of <see cref="HelpCommands"/> class.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="userApi">The user API.</param>
        /// <param name="responder">The command responder.</param>
        /// <param name="mediator">The mediator.</param>
        public HelpCommands(
            MessageContext context,
            IDiscordRestChannelAPI channelApi,
            IDiscordRestUserAPI userApi,
            ICommandResponder responder,
            IMediator mediator
        )
        {
            _context = context;
            _channelApi = channelApi;
            _userApi = userApi;
            _responder = responder;
            _mediator = mediator;
        }

        [Command("help")]
        [Description("Shows available commands")]
        public async Task<IResult> Help()
        {
            var getEmbedsResult = await _mediator.Send(new GetForAll.Query());
            if (!getEmbedsResult.IsSuccess)
            {
                return getEmbedsResult;
            }

            // this might create problems in the future, I'm not sure;
            var createDmResult = await _userApi.CreateDMAsync(_context.User.ID);
            if (!createDmResult.IsSuccess)
            {
                return createDmResult;
            }

            var sendEmbedResult =
                await _channelApi.CreateMessageAsync(createDmResult.Entity.ID, embeds: getEmbedsResult.Entity.ToList());
            if (!sendEmbedResult.IsSuccess)
            {
                return sendEmbedResult;
            }

            return await _responder.Respond("Help has been sent to your DMs :smile:.");
        }

        [Command("help")]
        [Description("Shows help for a given category")]
        public async Task<IResult> Help(string categoryName) =>
            await _mediator.Send(new GetHelpForCategory.Query(categoryName)) switch
            {
                { IsSuccess: true, Entity: { } embed } =>
                    await _responder.Respond(embed),

                { IsSuccess: true } =>
                    Result.FromError(new NotFoundError($"Could not find a category with name `{categoryName}`.")),

                { IsSuccess: false } res => res
            };
    }
}