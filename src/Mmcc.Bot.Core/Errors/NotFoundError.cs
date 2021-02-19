﻿using Remora.Commands.Trees.Nodes;
using Remora.Results;

namespace Mmcc.Bot.Core.Errors
{
    /// <summary>
    /// Represents a failure to find a resource.
    /// </summary>
    public record NotFoundError(string Message, IChildNode? Node = default) : ResultError(Message);
}