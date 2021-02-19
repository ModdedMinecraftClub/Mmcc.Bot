using System.Threading.Tasks;
using Mmcc.Bot.Core.Errors;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups
{
    [Group("apps")]
    public class ApplicationCommands : CommandGroup
    {
        public class ViewCommands : CommandGroup
        {
            private readonly ICommandContext _context;
            private readonly IDiscordRestChannelAPI _channelApi;

            public ViewCommands(ICommandContext context, IDiscordRestChannelAPI channelApi)
            {
                _context = context;
                _channelApi = channelApi;
            }
            
            [Command("view")]
            public async Task<IResult> View(int id)
            {
                var reply = await _channelApi.CreateMessageAsync(
                    _context.ChannelID,
                    "Sent from Remora",
                    ct: CancellationToken
                );
                return Result.FromError(new NotFoundError("error"));
                return reply;
            }
        }
    }
}