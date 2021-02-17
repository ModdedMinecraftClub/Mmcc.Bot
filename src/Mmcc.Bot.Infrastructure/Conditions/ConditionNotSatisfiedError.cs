﻿using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Conditions
{
    /// <summary>
    /// Represents a failure to satisfy a condition.
    /// </summary>
    public record ConditionNotSatisfiedError(string Message, IChildNode? Node = default) : ResultError(Message);
}