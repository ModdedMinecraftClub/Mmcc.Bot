using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Common.Errors;
using Mmcc.Bot.Common.Extensions.FluentValidation.Results;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Models.Settings;
using Mmcc.Bot.Common.Statics;
using Remora.Commands.Results;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Services;
using Remora.Results;

namespace Mmcc.Bot.Middleware;

public class ErrorNotificationMiddleware : IPostExecutionEvent
{
    private readonly ILogger<ErrorNotificationMiddleware> _logger;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IColourPalette _colourPalette;
    private readonly DiscordSettings _discordSettings;
    
    public ErrorNotificationMiddleware(
        ILogger<ErrorNotificationMiddleware> logger,
        IDiscordRestChannelAPI channelApi,
        IColourPalette colourPalette,
        DiscordSettings discordSettings
    )
    {
        _logger = logger;
        _channelApi = channelApi;
        _colourPalette = colourPalette;
        _discordSettings = discordSettings;
    }

    public async Task<Result> AfterExecutionAsync(
        ICommandContext context,
        IResult executionResult,
        CancellationToken ct
    )
    {
        if (executionResult.IsSuccess)
        {
            return Result.FromSuccess();
        }

        var err = executionResult.Error;
        var errorEmbed = new Embed
        {
            Thumbnail = EmbedProperties.MmccLogoThumbnail,
            Colour = _colourPalette.Red,
            Timestamp = DateTimeOffset.UtcNow
        };
        errorEmbed = err switch
        {
            CommandNotFoundError cnfe => errorEmbed with
            {
              Title  = ":exclamation: Command not found",
              Description = $"Could not find a matching command for `{_discordSettings.Prefix}{cnfe.OriginalInput}`."
            },
            ValidationError(var message, var readOnlyList, _) => errorEmbed with
            {
                Title = ":exclamation: Validation error.",
                Description = message.Replace('\'', '`'),
                Fields = new List<EmbedField> {readOnlyList.ToEmbedField()}
            },
            NotFoundError => errorEmbed with
            {
                Title = ":x: Resource not found.",
                Description = err.Message
            },
            null => errorEmbed with
            {
                Title = ":exclamation: Error.",
                Description = "Unknown error."
            },
            _ => errorEmbed with
            {
                Title = $":x: {err.GetType()}.",
                Description = err.Message
            }
        };

        var sendEmbedResult =
            await _channelApi.CreateMessageAsync(context.ChannelID, embeds: new[] { errorEmbed }, ct: ct);
        return !sendEmbedResult.IsSuccess
            ? Result.FromError(sendEmbedResult)
            : Result.FromSuccess();
    }
}