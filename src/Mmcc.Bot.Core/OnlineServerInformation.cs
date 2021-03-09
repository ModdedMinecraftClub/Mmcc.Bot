using System.Collections.Generic;

namespace Mmcc.Bot.Core
{
    public record OnlineServerInformation(
        string ServerId,
        string ServerName,
        string ServerAddress,
        int MaxPlayers,
        int PlayersOnline,
        List<string> OnlinePlayerNames
    );
}