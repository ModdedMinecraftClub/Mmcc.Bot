using System;

namespace Mmcc.Bot.Core
{
    public struct PolychatMessageString : IEquatable<PolychatMessageString>
    {
        /// <summary>
        /// Represents the empty string. This field is read only.
        /// </summary>
        public static readonly PolychatMessageString Empty = new(string.Empty);

        /// <summary>
        /// Creates a new <see cref="PolychatMessageString"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The raw value.</param>
        public PolychatMessageString(string? value)
        {
            Value = value ?? string.Empty;
        }

        /// <summary>
        /// Creates a new <see cref="PolychatMessageString"/> with the given <paramref name="serverId"></paramref> server ID and <paramref name="messageBody"></paramref> message body. 
        /// </summary>
        /// <param name="serverId">Server ID.</param>
        /// <param name="messageBody">Message body.</param>
        public PolychatMessageString(string serverId, string? messageBody)
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
        public bool HasValue => !string.IsNullOrEmpty(Value);

        /// <inheritdoc />
        public override string ToString() => HasValue ? new string(Value) : string.Empty;

        /// <summary>
        /// To a string that is formatted for Discord.
        /// </summary>
        /// <returns></returns>
        public string ToDiscordFormattedString()
        {
            var val = HasValue ? Value : " ";
            return $"`{val}`";
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) =>
            obj switch
            {
                null => !HasValue,
                PolychatMessageString other => Equals(other),
                _ => false
            };

        /// <inheritdoc />
        public bool Equals(PolychatMessageString other) =>
            !(HasValue || other.HasValue)
            || MemoryExtensions.Equals(Value, other.Value);

        /// <inheritdoc />
        public override int GetHashCode()
            => HasValue
                ? string.GetHashCode(Value)
                : 0;
        
        public static bool operator ==(PolychatMessageString left, PolychatMessageString right)
            => left.Equals(right);
        
        public static bool operator !=(PolychatMessageString left, PolychatMessageString right)
            => !left.Equals(right);
    }
}