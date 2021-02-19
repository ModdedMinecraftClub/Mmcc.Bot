using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Core.Statics;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Services
{
    /// <summary>
    /// Service that handles notifying the user that the command has failed.
    /// </summary>
    public class ErrorNotificationService : IExecutionEventService
    {
        private readonly ILogger<ErrorNotificationService> _logger;
        private readonly IDiscordRestChannelAPI _channelApi;
        
        /// <summary>
        /// Instantiates a new instance of <see cref="ErrorNotificationService"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="channelApi">The channel API.</param>
        public ErrorNotificationService(ILogger<ErrorNotificationService> logger, IDiscordRestChannelAPI channelApi)
        {
            _logger = logger;
            _channelApi = channelApi;
        }
        
        /// <inheritdoc />
        public Task<Result> BeforeExecutionAsync(ICommandContext context, CancellationToken ct = new CancellationToken())
        {
            return Task.FromResult(Result.FromSuccess());
        }
        
        /// <inheritdoc />
        public async Task<Result> AfterExecutionAsync(
            ICommandContext context,
            IResult executionResult,
            CancellationToken ct
            )
        {
            if (
                executionResult.IsSuccess
                || executionResult.Inner is null
                || executionResult.Inner.IsSuccess
            )
            {
                return Result.FromSuccess();
            }

            var err = executionResult.Inner.Error;
            var embedImg = new EmbedThumbnail(
                Urls.MmccLogoUrl,
                new Optional<string>(),
                new Optional<int>(),
                new Optional<int>()
            );
            
            var errorEmbed = new Embed(Thumbnail: embedImg, Colour: Color.Red);
            errorEmbed = err switch
            {
                null            => errorEmbed with
                                    {
                                        Title = ":exclamation: Error.",
                                        Description = "Unknown error."
                                    },
                ValidationError => errorEmbed with
                                    {
                                        Title = ":exclamation: Validation error.",
                                        Description = err.Message
                                    },
                NotFoundError   => errorEmbed with
                                    {
                                        Title = ":x: Resource not found.",
                                        Description = err.Message
                                    },
                _               => errorEmbed with
                                    {
                                        Title = $":x: {err.GetType()} error.",
                                        Description = err.Message
                                    }
            };

            var sendEmbedResult = await _channelApi.CreateMessageAsync(context.ChannelID, content: "something",
                isTTS: false, embed: errorEmbed, ct: ct);
            return !sendEmbedResult.IsSuccess
                ? Result.FromError(sendEmbedResult)
                : Result.FromSuccess();
        }
    }
}