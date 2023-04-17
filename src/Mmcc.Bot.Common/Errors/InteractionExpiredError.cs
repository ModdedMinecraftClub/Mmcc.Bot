using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Mmcc.Bot.Common.Errors;

/// <summary>
/// Represents a failure resulting from an interaction having expired in the corresponding store.
/// </summary>
public record InteractionExpiredError(string Message, IChildNode? Node = default) : ResultError(Message);