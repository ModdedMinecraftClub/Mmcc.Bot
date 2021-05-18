using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Core;
using Mmcc.Bot.Core.Extensions;

namespace Mmcc.Bot.Infrastructure.Services
{
    /// <summary>
    /// Polychat service.
    /// </summary>
    public interface IPolychatService
    {
        /// <summary>
        /// Tries to get the online server with the specified ID.
        /// </summary>
        /// <param name="id">ID of the server.</param>
        /// <returns>Online server associated with the specified ID or default in case of a failure.</returns>
        OnlineServer? GetOnlineServerOrDefault(string id);
        
        /// <summary>
        /// Adds or updates an online server.
        /// </summary>
        /// <param name="id">ID of the server to add/update.</param>
        /// <param name="onlineServer">The server object.</param>
        /// <returns>The added/updated server object.</returns>
        OnlineServer AddOrUpdateOnlineServer(string id, OnlineServer onlineServer);
        
        /// <summary>
        /// Removes an online server.
        /// </summary>
        /// <param name="id">ID of the server to remove.</param>
        void RemoveOnlineServer(string id);
        
        /// <summary>
        /// Sends a protobuf message to a server.
        /// </summary>
        /// <param name="destinationServer">Destination server.</param>
        /// <param name="packedMsgBytes">Protobuf message packed as <see cref="Any"/> in a <see cref="byte"/> <see cref="Array"/> form to be sent.</param>
        void SendMessage(OnlineServer destinationServer, byte[] packedMsgBytes);
        
        /// <summary>
        /// Sends a protobuf message to a server.
        /// </summary>
        /// <param name="destinationServer">Destination server.</param>
        /// <param name="message">Protobuf message packed as <see cref="Any"/> to be sent.</param>
        void SendMessage(OnlineServer destinationServer, Any message);
        
        /// <summary>
        /// Sends a protobuf message to a server.
        /// </summary>
        /// <param name="destinationServer">Destination server.</param>
        /// <param name="message">Protobuf message to be sent.</param>
        /// <typeparam name="T">Message type. Must be a Protobuf message, meaning it must implement <see cref="IMessage{T}"/>.</typeparam>
        void SendMessage<T>(OnlineServer destinationServer, T message) where T : IMessage<T>;
        
        /// <summary>
        /// Forwards a protobuf message.
        /// </summary>
        /// <param name="authorId">ID of the server the message has originated from.</param>
        /// <param name="message">Protobuf message to be sent.</param>
        /// <typeparam name="T">Message type. Must be a Protobuf message, meaning it must implement <see cref="IMessage{T}"/>.</typeparam>
        /// <remarks>To forward a message means to send a message to all the online servers apart from the server the message has originated from.</remarks>
        void ForwardMessage<T>(string authorId, T message) where T : IMessage<T>;
        
        /// <summary>
        /// Broadcasts a protobuf message.
        /// </summary>
        /// <param name="message">Protobuf message to be sent.</param>
        /// <typeparam name="T">Message type. Must be a Protobuf message, meaning it must implement <see cref="IMessage{T}"/>.</typeparam>
        /// <remarks>To broadcast a message means to send a message to all the online servers.</remarks>
        void BroadcastMessage<T>(T message) where T : IMessage<T>;

        /// <summary>
        /// Gets information about online servers.
        /// </summary>
        /// <returns>Information about online servers.</returns>
        IEnumerable<OnlineServerInformation> GetInformationAboutOnlineServers();
    }
    
    /// <inheritdoc />
    public class PolychatService : IPolychatService
    {
        private readonly ILogger<PolychatService> _logger;
        private readonly ConcurrentDictionary<string, OnlineServer> _onlineServers;

        /// <summary>
        /// Instantiates a new instance of <see cref="PolychatService"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public PolychatService(ILogger<PolychatService> logger)
        {
            _logger = logger;
            _onlineServers = new();
        }

        /// <inheritdoc />
        public OnlineServer? GetOnlineServerOrDefault(string id) => _onlineServers.GetValueOrDefault(id);

        /// <inheritdoc />
        public OnlineServer AddOrUpdateOnlineServer(string id, OnlineServer onlineServer) =>
            _onlineServers.AddOrUpdate(id, onlineServer, (_, _) => onlineServer);

        /// <inheritdoc />
        public void RemoveOnlineServer(string id)
        {
            var removeIsSuccess = _onlineServers.TryRemove(id, out _);
            
            if (removeIsSuccess)
            {
                _logger.LogInformation("Removed server with {id} from the list of online servers", id);
            }
            else
            {
                _logger.LogError("Failed to remove server with {id} from the list of online servers", id);
            }
        }

        /// <inheritdoc />
        public void SendMessage(OnlineServer destinationServer, byte[] packedMsgBytes)
        {
            try
            {
                // if this bit of code ever goes tits up with a NullReferenceException, please send an email to
                // john01dav@gmail.com as he has personally assured me that ConnectedClient
                // can't be null (Discord message ID: 818759362839183360);
                destinationServer.ConnectedClient!.SendMessage(packedMsgBytes);
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to send a message to server with {destinationServer.ServerId}", e);
            }
        }

        /// <inheritdoc />
        public void SendMessage(OnlineServer destinationServer, Any message) =>
            SendMessage(destinationServer, message.ToByteArray());

        /// <inheritdoc />
        public void SendMessage<T>(OnlineServer destinationServer, T message) where T : IMessage<T> =>
            SendMessage(destinationServer, Any.Pack(message).ToByteArray());

        /// <inheritdoc />
        public void ForwardMessage<T>(string authorId, T message) where T : IMessage<T>
        {
            var packedMsgBytes = Any.Pack(message).ToByteArray();

            foreach (var (_, onlineServer) in _onlineServers)
            {
                if (onlineServer.ServerId.Equals(authorId)) break;

                SendMessage(onlineServer, packedMsgBytes);
            }
        }

        /// <inheritdoc />
        public void BroadcastMessage<T>(T message) where T : IMessage<T>
        {
            var packedMsgBytes = Any.Pack(message).ToByteArray();

            foreach (var (_, onlineServer) in _onlineServers)
            {
                SendMessage(onlineServer, packedMsgBytes);
            }
        }

        /// <inheritdoc />
        public IEnumerable<OnlineServerInformation> GetInformationAboutOnlineServers()
        {
            foreach (var (_, onlineServer) in _onlineServers)
            {
                yield return onlineServer.ExtractServerInformation();
            }
        }
    }
}