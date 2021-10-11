namespace Mmcc.Bot.Database.Settings;

/// <summary>
/// Settings for the underlying MySQL database.
/// </summary>
public class MySqlSettings
{
    /// <summary>
    /// MySQL/MariaDB version string, e.g. "10.4.11-mariadb" 
    /// </summary>
    public string MySqlVersionString { get; set; } = null!;
        
    /// <summary>
    /// How many times failed SQL commands should be retried.
    /// </summary>
    ///
    /// <remarks>The recommended value is 3.</remarks>
    public int RetryAmount { get; set; }
        
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

    /// <summary>
    /// The formatted MySQL connection string.
    /// </summary>
    public string ConnectionString =>
        $"Server={ServerIp};Port={Port};Database={DatabaseName};Uid={Username};Pwd={Password};Allow User Variables=True";
}