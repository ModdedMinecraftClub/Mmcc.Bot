using System;
using System.Text.RegularExpressions;

namespace Mmcc.Bot.Core
{
    /// <summary>
    /// Represents a polychat chat message string.
    /// </summary>
    public struct PolychatChatMessageString : IEquatable<PolychatChatMessageString>
    {
        /// <summary>
        /// Represents the empty string. This field is read-only.
        /// </summary>
        public static readonly PolychatChatMessageString Empty = new(string.Empty);

        /// <summary>
        /// Creates a new <see cref="PolychatChatMessageString"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The raw value.</param>
        public PolychatChatMessageString(string? value) => 
            Value = value ?? string.Empty;

        /// <summary>
        /// Creates a new <see cref="PolychatChatMessageString"/> with the given <paramref name="serverId"></paramref> server ID and <paramref name="messageBody"></paramref> message body. 
        /// </summary>
        /// <param name="serverId">Server ID.</param>
        /// <param name="messageBody">Message body.</param>
        public PolychatChatMessageString(string serverId, string? messageBody)
        {
            var msg = messageBody ?? string.Empty;
            Value = $"[{serverId.ToUpperInvariant()}] {msg}";
        }
        
        /// <summary>
        /// The value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets whether <see cref="Value"/> has a meaningful value.
        /// </summary>
        public bool HasValue => 
            !string.IsNullOrEmpty(Value);

        /// <inheritdoc />
        public override string ToString() => 
            HasValue ? new string(Value) : string.Empty;

        /// <summary>
        /// To a string that is formatted for Discord.
        /// </summary>
        /// <returns></returns>
        public string ToDiscordFormattedString()
        {
            var val = HasValue 
                ? Regex.Replace(Value, "§.", "").Replace('`', '\'')
                : " ";
            
            return $"`{val}`";
        }

        public string ToSanitisedString() =>
            HasValue
                ? Regex.Replace(Value, "§.", "").Replace('`', '\'')
                : " ";

        /// <inheritdoc />
        public override bool Equals(object? obj) =>
            obj switch
            {
                null => !HasValue,
                PolychatChatMessageString other => Equals(other),
                _ => false
            };

        /// <inheritdoc />
        public bool Equals(PolychatChatMessageString other) =>
            !(HasValue || other.HasValue)
            || MemoryExtensions.Equals(Value, other.Value);

        /// <inheritdoc />
        public override int GetHashCode()
            => HasValue
                ? string.GetHashCode(Value)
                : 0;
        
        public static bool operator ==(PolychatChatMessageString left, PolychatChatMessageString right)
            => left.Equals(right);
        
        public static bool operator !=(PolychatChatMessageString left, PolychatChatMessageString right)
            => !left.Equals(right);
    }
}