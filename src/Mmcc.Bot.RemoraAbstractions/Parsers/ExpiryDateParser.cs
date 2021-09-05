using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.Common.Models;
using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Parsers
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
        public override async ValueTask<Result<ExpiryDate>> TryParseAsync(string value, CancellationToken ct = default)
        {
            if (PermanentAliases.Contains(value.ToLowerInvariant()))
            {
                return new ExpiryDate();
            }

            var timeSpanParser = new TimeSpanParser();
            var parseTimeSpanResult = await timeSpanParser.TryParseAsync(value, ct);

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