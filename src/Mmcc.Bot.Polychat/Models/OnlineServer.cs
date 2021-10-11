using System.Collections.Generic;
using Ssmp;

namespace Mmcc.Bot.Polychat.Models;

/// <summary>
/// Represents an online MC server.
/// </summary>
public class OnlineServer
{
    /// <summary>
    /// ID of the server.
    /// </summary>
    public string ServerId { get; }
        
    /// <summary>
    /// Human friendly name of the server.
    /// </summary>
    public string ServerName { get; }
        
    /// <summary>
    /// IP address of the server.
    /// </summary>
    public string ServerAddress { get; }
        
    /// <summary>
    /// Maximum amount of players that can be on the server at once.
    /// </summary>
    public int MaxPlayers { get; }
        
    /// <summary>
    /// Amount of players that are currently online on the server.
    /// </summary>
    public int PlayersOnline { get; set; }
        
    /// <summary>
    /// List of in-game names of all the players that are currently online on the server. 
    /// </summary>
    public List<string> OnlinePlayerNames { get; set; }
        
    /// <summary>
    /// The Ssmp TCP client corresponding to the server.  
    /// </summary>
    public ConnectedClient ConnectedClient { get; }

    /// <summary>
    /// Instantiates a new instance of <see cref="OnlineServer"/>.
    /// </summary>
    /// <param name="serverInfo">Info message sent by the server when it comes online.</param>
    /// <param name="connectedClient">The Ssmp TCP client corresponding to the server.  </param>
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

    public OnlineServerInformation ExtractServerInformation() =>
        new(ServerId, ServerName, ServerAddress, MaxPlayers, PlayersOnline, OnlinePlayerNames);
}