using System;
using System.Threading;
using System.Threading.Tasks;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Services
{
    public class DiscordErrorMessageService : IExecutionEventService
    {
        private readonly IDiscordRestChannelAPI _channelApi;

        public DiscordErrorMessageService(IDiscordRestChannelAPI channelApi)
        {
            _channelApi = channelApi;
        }

        public Task<Result> BeforeExecutionAsync(ICommandContext context, CancellationToken ct)
        {
            return Task.FromResult(Result.FromSuccess());
        }

        public async Task<Result> AfterExecutionAsync(ICommandContext context, IResult executionResult, CancellationToken ct)
        {
            if (executionResult.IsSuccess) return Result.FromSuccess();
            await _channelApi.CreateMessageAsync(context.ChannelID, executionResult.Error?.Message, ct: ct);
            
            return Result.FromSuccess();
        }
    }
}