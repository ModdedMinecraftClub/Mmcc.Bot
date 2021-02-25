using System;
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
    /// Parses instances of <see cref="ExpiryDate"/> from command inputs.
    /// </summary>
    public class ExpiryDateParser : AbstractTypeParser<ExpiryDate>
    {
        /// <inheritdoc />
        public override ValueTask<Result<ExpiryDate>> TryParse(string value, CancellationToken ct)
        {
            if (value.Equals("perm") || value.Equals("p"))
            {
                return new(new ExpiryDate());
            }
            
            // regex obtained from a back alley code dealer;
            var match = Regex.Match(value, "([1-9][0-9]+)([mhd])");

            if (!match.Success)
            {
                return new(new ParsingError<ExpiryDate>(
                    $"Could not parse input \"{value}\" into a valid {nameof(ExpiryDate)}"));
            }

            var amountString = match.Groups[1].Value;
            var c = match.Groups[2].Value;
            var parseAmountResult = int.TryParse(amountString, out var amount);

            if (!parseAmountResult)
            {
                return new(new ParsingError<ExpiryDate>(
                    $"Could not parse matches amount \"{amountString}\" into a valid {nameof(Int32)}"));
            }

            DateTimeOffset dateTimeOffset;
            try
            {
                dateTimeOffset = c switch
                {
                    "m" => DateTimeOffset.UtcNow.AddMinutes(amount),
                    "h" => DateTimeOffset.UtcNow.AddHours(amount),
                    "d" => DateTimeOffset.UtcNow.AddDays(amount),
                    _ => throw new ArgumentOutOfRangeException(c)
                };
            }
            catch(ArgumentOutOfRangeException e)
            {
                return new(new ParsingError<ExpiryDate>($"{e.Message}"));
            } 

            return new(new ExpiryDate {Value = dateTimeOffset.ToUnixTimeMilliseconds()});
        }
    }
}