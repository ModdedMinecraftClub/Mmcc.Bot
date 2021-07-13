using System;
using System.Text.RegularExpressions;

namespace Mmcc.Bot.Polychat.Abstractions
{
    /// <summary>
    /// Represents a polychat server ID.
    /// </summary>
    public struct PolychatServerIdString : IEquatable<PolychatServerIdString>
    {
        /// <summary>
        /// Represents the empty string. This field is read-only.
        /// </summary>
        public static readonly PolychatServerIdString Empty = new(string.Empty);

        /// <summary>
        /// Creates a new <see cref="PolychatServerIdString"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The raw value.</param>
        public PolychatServerIdString(string? value) =>
            Value = value ?? string.Empty;
        
        /// <summary>
        /// The value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets whether <see cref="Value"/> has a meaningful value.
        /// </summary>
        public bool HasValue =>
            !string.IsNullOrEmpty(Value);

        /// <summary>
        /// Gets the sanitised version of the ID.
        /// </summary>
        /// <returns>Sanitised ID.</returns>
        public string ToSanitised() =>
            HasValue
                ? Regex.Replace(Regex.Replace(Value, "§.", ""), "[\\[\\]]", "")
                : string.Empty;
        
        /// <summary>
        /// Gets the sanitised version of the ID uppercase.
        /// </summary>
        /// <returns>Sanitised ID uppercase.</returns>
        public string ToSanitisedUppercase() =>
            HasValue
                ? Regex.Replace(Regex.Replace(Value, "§.", ""), "[\\[\\]]", "").ToUpper()
                : string.Empty;
        
        /// <inheritdoc />
        public override bool Equals(object? obj) =>
            obj switch
            {
                null => !HasValue,
                PolychatServerIdString other => Equals(other),
                _ => false
            };
        
        /// <inheritdoc />
        public bool Equals(PolychatServerIdString other) =>
            !(HasValue || other.HasValue)
            || MemoryExtensions.Equals(Value, other.Value);
        
        /// <inheritdoc />
        public override int GetHashCode()
            => HasValue
                ? string.GetHashCode(Value)
                : 0;
        
        public static bool operator ==(PolychatServerIdString left, PolychatServerIdString right)
            => left.Equals(right);
        
        public static bool operator !=(PolychatServerIdString left, PolychatServerIdString right)
            => !left.Equals(right);
    }
}