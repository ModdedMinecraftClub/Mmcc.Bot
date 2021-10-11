﻿using System.Collections.Generic;

namespace Mmcc.Bot.Polychat.Models;

/// <summary>
/// Represents information about an online MC server.
/// </summary>
public record OnlineServerInformation(
    string ServerId,
    string ServerName,
    string ServerAddress,
    int MaxPlayers,
    int PlayersOnline,
    List<string> OnlinePlayerNames
);