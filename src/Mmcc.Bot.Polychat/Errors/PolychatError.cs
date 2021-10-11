using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Mmcc.Bot.Polychat.Errors;

/// <summary>
/// Represents a failure to communicate with polychat2's central server.
/// </summary>
public record PolychatError(string Message, IChildNode? Node = default) : ResultError(Message);