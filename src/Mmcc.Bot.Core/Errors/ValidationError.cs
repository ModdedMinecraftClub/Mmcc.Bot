using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Mmcc.Bot.Core.Errors
{
    /// <summary>
    /// Represents a parameter validation error.
    /// </summary>
    public record ValidationError(string Message, IChildNode? Node = default) : ResultError(Message);
}