namespace Mmcc.Bot.Core.Models.Settings
{
    /// <summary>
    /// Settings for communication with Polychat2.
    /// </summary>
    public class PolychatSettings
    {
        /// <summary>
        /// IP of the central Polychat2 server.
        /// </summary>
        public string ServerIp { get; set; } = null!;
        
        /// <summary>
        /// Port of the central Polychat2 server.
        /// </summary>
        public int Port { get; set; }
        
        /// <summary>
        /// Buffer size for the proto messages.
        /// </summary>
        public int BufferSize { get; set; }
    }
}