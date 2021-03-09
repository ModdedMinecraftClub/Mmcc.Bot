using System.Threading.Tasks;
using Ssmp;

namespace Mmcc.Bot.Infrastructure.Services
{
    /// <summary>
    /// Service for processing incoming TCP messages.
    /// </summary>
    public interface ITcpMessageProcessingService
    {
        /// <summary>
        /// Handles an incoming TCP message.
        /// </summary>
        /// <param name="connectedClient">Author of the message.</param>
        /// <param name="message">Message as a byte array.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        Task Handle(ConnectedClient connectedClient, byte[] message);
    }
}