namespace Mmcc.Bot.Core.Extensions
{
    public static class OnlineServerExtensions
    {
        public static OnlineServerInformation ExtractServerInformation(this OnlineServer onlineServer) =>
            new(onlineServer.ServerId, onlineServer.ServerName, onlineServer.ServerAddress,
                onlineServer.MaxPlayers, onlineServer.PlayersOnline, onlineServer.OnlinePlayerNames);
    }
}