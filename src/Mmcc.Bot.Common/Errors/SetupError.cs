using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Mmcc.Bot.Common.Errors
{
    /// <summary>
    /// Represents a failure to setup the bot in a guild.
    /// </summary>
    public record SetupError(string Message, IChildNode? Node = default) : ResultError(Message);
}