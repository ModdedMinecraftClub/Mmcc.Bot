using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.Common.Models;
using Remora.Commands.Parsers;
using Remora.Commands.Results;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Parsers;

/// <summary>
/// Parses instances of <see cref="DiscordMemberApplication"/>.
/// </summary>
public class DiscordMemberApplicationParser : AbstractTypeParser<DiscordMemberApplication>
{
    private static readonly string[] ServerAliases =
    {
        "server", "pack"
    };
        
    private static readonly string[] IgnAliases =
    {
        "ign", "name", "username", "igns", "names", "usernames"
    };

    public override async ValueTask<Result<DiscordMemberApplication>> TryParseAsync(string s, CancellationToken ct = default)
    {
        if (!s.Contains('\n'))
        {
            return new ParsingError<DiscordMemberApplication>(
                $"Could not parse `{s}` into a valid {nameof(DiscordMemberApplication)} because it did not contain multiple lines.");
        }

        string? server = default;
        List<string>? igns = default;
        using (var reader = new StringReader(s))
        {
            string? l;
            while ((l = await reader.ReadLineAsync()) is not null)
            {
                if (!l.Contains(':'))
                {
                    continue;
                }
                    
                // actual value (i.e. text after ':');
                var value = l[(l.IndexOf(':') + 1)..].Trim();
                    
                if (ServerAliases.Any(a => l.Contains(a, StringComparison.OrdinalIgnoreCase)))
                {
                    server = value;
                }
                else if (IgnAliases.Any(a => l.Contains(a, StringComparison.OrdinalIgnoreCase)))
                {
                    igns = value.Split(',').Select(i => i.Trim()).ToList();
                }
            }
        }

        if (string.IsNullOrWhiteSpace(server))
        {
            return new ParsingError<DiscordMemberApplication>(
                $"Could not parse `{s}` into a valid {nameof(DiscordMemberApplication)} because it did not contain a valid server.");
        }

        if (igns is null || !igns.Any())
        {
            return new ParsingError<DiscordMemberApplication>(
                $"Could not parse `{s}` into a valid {nameof(DiscordMemberApplication)} because it did not contain valid IGNs.");
        }

        return Result<DiscordMemberApplication>.FromSuccess(new DiscordMemberApplication(server, igns));
    }
}