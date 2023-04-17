using System;
using System.Collections.Generic;
using MediatR;

namespace Mmcc.Bot.Common.Models;

public interface IMmccNotification : INotification
{
    public string Title { get; }
    public string? Description { get; }
    public DateTimeOffset? Timestamp { get; }
    IReadOnlyList<KeyValuePair<string, string>>? CustomProperties { get; }
}