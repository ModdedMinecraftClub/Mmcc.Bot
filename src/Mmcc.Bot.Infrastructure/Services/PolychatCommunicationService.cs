using System;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Core.Models.Settings;
using Remora.Results;

namespace Mmcc.Bot.Infrastructure.Services
{
    /// <summary>
    /// Service used for communication with polychat2's central server over TCP.
    /// </summary>
    [Obsolete("Do not send messages to central server over TCP as it is now in-process. Send via PolychatService instead.")]
    public interface IPolychatCommunicationService
    {
        /// <summary>
        /// Sends a protobuf message to polychat2's central server over TCP.
        /// </summary>
        /// <param name="protobufMessage"></param>
        /// <returns>Result of the operation.</returns>
        public Task<Result> SendProtobufMessage(IMessage protobufMessage);
    }
    
    /// <inheritdoc />
    [Obsolete("Do not send messages to central server over TCP as it is now in-process. Send via PolychatService instead.")]
    public class PolychatCommunicationService : IPolychatCommunicationService
    {
        private readonly ILogger<PolychatCommunicationService> _logger;
        private readonly PolychatSettings _polychatSettings;
        
        /// <summary>
        /// Instantiates a new instance of <see cref="PolychatCommunicationService"/>.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="polychatSettings">The polychat settings.</param>
        public PolychatCommunicationService(ILogger<PolychatCommunicationService> logger, PolychatSettings polychatSettings)
        {
            _logger = logger;
            _polychatSettings = polychatSettings;
        }
        
        /// <inheritdoc />
        public async Task<Result> SendProtobufMessage(IMessage protobufMessage)
        {
            try
            {
                _logger.LogInformation("Opening the connection with polychat2's central server.");
                
                using var client = new TcpClient(AddressFamily.InterNetwork);
                await client.ConnectAsync(_polychatSettings.ServerIp, _polychatSettings.Port);
                await using var stream = client.GetStream();
                var packedMsg = Any.Pack(protobufMessage);
                var lengthArrayBuffer = new byte[4];
                var msgBytes = packedMsg.ToByteArray();

                BinaryPrimitives.WriteInt32BigEndian(lengthArrayBuffer.AsSpan(), msgBytes.Length);
                
                _logger.LogInformation("Sending the message to polychat2's central server.");
                
                await stream.WriteAsync(lengthArrayBuffer);
                await stream.WriteAsync(msgBytes);

                _logger.LogInformation("Successfully sent the message to polychat2's central server.");
                return Result.FromSuccess();
            }
            catch (Exception e)
            {
                _logger.LogError("Error while communicating with polychat2",e);
                return e;
            }
        }
    }
}