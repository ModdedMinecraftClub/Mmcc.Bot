using Remora.Discord.Core;

namespace Mmcc.Bot.Core.Models.MojangApi
{
    /// <summary>
    /// Represents player name info.
    /// </summary>
    public interface IPlayerNameInfo
    {
        /// <summary>
        /// Player username.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// When the user changed their username to the given username.
        /// </summary>
        long? ChangedToAt { get; }
    }
    
    /// <inheritdoc cref="IPlayerNameInfo"/>
    public record PlayerNameInfo
    (
        string Name,
        long? ChangedToAt
    ) : IPlayerNameInfo;
}