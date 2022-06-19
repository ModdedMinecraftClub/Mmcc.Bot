using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Mmcc.Bot.Polychat.Errors;

/// <summary>
/// Represents an error raised on an unknown Protocol Buffer message.
/// </summary>
public record UnknownMessageType(string Message, IChildNode? Node = default) : ResultError(Message);