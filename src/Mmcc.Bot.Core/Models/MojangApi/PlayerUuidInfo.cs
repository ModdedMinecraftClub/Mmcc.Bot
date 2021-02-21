using Remora.Discord.Core;

namespace Mmcc.Bot.Core.Models.MojangApi
{
    /// <summary>
    /// Represents player UUID info.
    /// </summary>
    public interface IPlayerUuidInfo
    {
        /// <summary>
        /// UUID of the player.
        /// </summary>
        public string Id { get; }
        
        /// <summary>
        /// Current username of the player.
        /// </summary>
        public string Name { get; }
    }

    /// <inheritdoc cref="IPlayerUuidInfo" />
    public record PlayerUuidInfo
    (
        string Id,
        string Name
    ) : IPlayerUuidInfo;
}