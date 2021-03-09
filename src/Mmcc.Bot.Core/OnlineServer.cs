using System.Collections.Generic;
using Mmcc.Bot.Protos;
using Ssmp;

namespace Mmcc.Bot.Core
{
    public class OnlineServer
    {
        public string ServerId { get; }
        public string ServerName { get; }
        public string ServerAddress { get; }
        public int MaxPlayers { get; }
        public int PlayersOnline { get; set; }
        public List<string> OnlinePlayerNames { get; set; }
        public ConnectedClient ConnectedClient { get; }

        public OnlineServer(ServerInfo serverInfo, ConnectedClient connectedClient)
        {
            ServerId = serverInfo.ServerId.ToUpperInvariant();
            ServerName = serverInfo.ServerName;
            ServerAddress = serverInfo.ServerAddress;
            MaxPlayers = serverInfo.MaxPlayers;
            PlayersOnline = 0;
            OnlinePlayerNames = new();
            ConnectedClient = connectedClient;
        }
    }
}