using System;
using System.Collections.Generic;
using MediatR;

namespace Mmcc.Bot.Common.Models;

public record Notification(
    string Title,
    string? Description,
    DateTimeOffset? Timestamp,
    IReadOnlyList<KeyValuePair<string, string>>? CustomProperties
) : INotification;
