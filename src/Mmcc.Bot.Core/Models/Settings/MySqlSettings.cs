namespace Mmcc.Bot.Core.Models.Settings
{
    /// <summary>
    /// Settings for the underlying MySQL database.
    /// </summary>
    public class MySqlSettings
    {
        /// <summary>
        /// IP of the database server.
        /// </summary>
        public string ServerIp { get; set; } = null!;
        
        /// <summary>
        /// Port of the database server.
        /// </summary>
        public int Port { get; set; }
        
        /// <summary>
        /// Name of the database for the bot.
        /// </summary>
        public string DatabaseName { get; set; } = null!;
        
        /// <summary>
        /// Username of the user via which the bot will access the database.
        /// </summary>
        public string Username { get; set; } = null!;
        
        /// <summary>
        /// Password of the user via which the bot will access the database.
        /// </summary>
        public string Password { get; set; } = null!;
    }
}