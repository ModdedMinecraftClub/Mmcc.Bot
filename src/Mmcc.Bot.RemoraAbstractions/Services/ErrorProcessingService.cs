using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using Mmcc.Bot.Common.Errors;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Models.Settings;
using Mmcc.Bot.Common.Statics;
using Remora.Commands.Results;
using Remora.Discord.API.Objects;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Services;

public interface IErrorProcessingService
{
    public Embed GetErrorEmbed(IResultError err);
}

public class ErrorProcessingService : IErrorProcessingService
{
    private readonly IColourPalette _colourPalette;
    private readonly DiscordSettings _discordSettings;

    public ErrorProcessingService(IColourPalette colourPalette, DiscordSettings discordSettings)
    {
        _colourPalette = colourPalette;
        _discordSettings = discordSettings;
    }

    public Embed GetErrorEmbed(IResultError err)
    {
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
            ValidationError(var message, var validationFailures, _) => errorEmbed with
            {
                Title = ":exclamation: Validation error.",
                Description = message.Replace('\'', '`'),
                Fields = new List<EmbedField> {ValidationFailuresToEmbedField(validationFailures)}
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

        return errorEmbed;
    }
    
    /// <summary>
    /// Gets <see cref="EmbedField"/> containing details of failures.
    /// </summary>
    /// <param name="validationFailures">Validation failures.</param>
    /// <param name="inline">Whether the <see cref="EmbedField"/> should be inline. Defaults to <code>false</code>.</param>
    /// <returns><see cref="EmbedField"/> containing details of failures.</returns>
    private static EmbedField ValidationFailuresToEmbedField(IEnumerable<ValidationFailure> validationFailures, bool inline = false)
    {
        var validationFailuresList = validationFailures.ToList();

        if (!validationFailuresList.Any())
        {
            return new("Failures", "No description.");
        }
            
        var descriptionSb = string.Join("\n",
            validationFailuresList
                .Select((vf, i) => $"{i + 1}) {vf.ToString().Replace('\'', '`')}"));
        return new("Reason(s)", descriptionSb, inline);
    }
}