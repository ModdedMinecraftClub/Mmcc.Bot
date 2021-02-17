using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups
{
    public class TestCommands : CommandGroup
    {
        private readonly ICommandContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;

        public TestCommands(ICommandContext context, IDiscordRestChannelAPI channelApi)
        {
            _context = context;
            _channelApi = channelApi;
        }
        
        [Command("test2")]
        [Description("testsasgagfad")]
        public async Task<IResult> Test2()
        {
            var reply = await _channelApi.CreateMessageAsync(
                _context.ChannelID,
                "Sent from Remora",
                ct: CancellationToken
            );

            return !reply.IsSuccess
                ? Result.FromError(reply)
                : Result.FromSuccess();
        }

        [Command("test")]
        [RequireContext(ChannelContext.Guild)]
        public async Task<IResult> Test()
        {
            var reply = await _channelApi.CreateMessageAsync(
                _context.ChannelID,
                "Sent from Remora",
                ct: CancellationToken
            );

            return !reply.IsSuccess
                ? Result.FromError(reply)
                : Result.FromSuccess();
        }

        [Command("restricted")]
        [RequireContext(ChannelContext.Guild)]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> Restricted()
        {
            var reply = await _channelApi.CreateMessageAsync(
                _context.ChannelID,
                "Sent restricted command from Remora",
                ct: CancellationToken
            );

            return !reply.IsSuccess
                ? Result.FromError(reply)
                : Result.FromSuccess();
        }
    }
}