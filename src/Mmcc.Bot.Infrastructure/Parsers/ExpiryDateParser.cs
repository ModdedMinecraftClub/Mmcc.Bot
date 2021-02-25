using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.Core.Models;
using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Parsers
{
    /// <summary>
    /// Parses instances of <see cref="ExpiryDate"/>.
    /// </summary>
    public class ExpiryDateParser : AbstractTypeParser<ExpiryDate>
    {
        private static readonly string[] PermanentAliases =
        {
            "perm",
            "perma",
            "permanent",
            "p"
        };

        /// <inheritdoc />
        public override async ValueTask<Result<ExpiryDate>> TryParse(string value, CancellationToken ct)
        {
            if (PermanentAliases.Contains(value.ToLowerInvariant()))
            {
                return new ExpiryDate();
            }

            var timeSpanParser = new TimeSpanParser();
            var parseTimeSpanResult = await timeSpanParser.TryParse(value, ct);

            if (!parseTimeSpanResult.IsSuccess)
            {
                return new ParsingError<ExpiryDate>(
                    $"Could not parse matches amount \"{value}\" into a valid {nameof(ExpiryDate)}");
            }

            var dateTimeOffset = DateTimeOffset.UtcNow.Add(parseTimeSpanResult.Entity);
            return new ExpiryDate {Value = dateTimeOffset.ToUnixTimeMilliseconds()};
        }
    }
}