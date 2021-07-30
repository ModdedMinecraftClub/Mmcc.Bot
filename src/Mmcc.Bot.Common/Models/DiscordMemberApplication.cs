using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Mmcc.Bot.Common.Models
{
    /// <summary>
    /// Represents a parsed Discord MMCC member application.
    /// </summary>
    /// <param name="IGNs">IGNs of the people applying</param>
    /// <param name="Server"></param>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public record DiscordMemberApplication(
        string Server,
        List<string> IGNs
    );
}