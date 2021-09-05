using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Mmcc.Bot.Common.Errors
{
    /// <summary>
    /// Represents a failure resulting from a property being null or missing.
    /// </summary>
    public record PropertyMissingOrNullError(string Message, IChildNode? Node = default) : ResultError(Message);
}