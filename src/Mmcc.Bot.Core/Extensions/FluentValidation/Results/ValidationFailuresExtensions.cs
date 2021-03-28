using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.Core.Extensions.FluentValidation.Results
{
    /// <summary>
    /// Extensions for <see cref="IEnumerable{ValidationFailure}"/>.
    /// </summary>
    public static class ValidationFailuresExtensions
    {
        /// <summary>
        /// Gets <see cref="EmbedField"/> containing details of failures.
        /// </summary>
        /// <param name="validationFailures">Validation failures.</param>
        /// <param name="inline">Whether the <see cref="EmbedField"/> should be inline. Defaults to <code>false</code>.</param>
        /// <returns><see cref="EmbedField"/> containing details of failures.</returns>
        public static EmbedField ToEmbedField(this IEnumerable<ValidationFailure> validationFailures, bool inline = false)
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
}