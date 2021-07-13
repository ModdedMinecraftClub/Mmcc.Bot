using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Mmcc.Bot.Mojang.Errors
{
    /// <summary>
    /// Represents a failure while trying to obtain data from Mojang API.
    /// </summary>
    public record MojangApiError(string Message, IChildNode? Node = default) : ResultError(Message);
}