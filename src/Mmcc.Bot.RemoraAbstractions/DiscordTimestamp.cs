using System;

namespace Mmcc.Bot.RemoraAbstractions
{
    /// <summary>
    /// Represents a Discord timestamp.
    /// </summary>
    public readonly struct DiscordTimestamp : IEquatable<DiscordTimestamp>
    {
        /// <summary>
        /// Creates a new <see cref="DiscordTimestamp"/> with the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        public DiscordTimestamp(DateTimeOffset value) =>
            Value = value;

        /// <summary>
        /// The value.
        /// </summary>
        public DateTimeOffset Value { get; }

        /// <summary>
        /// Gets the unstyled Discord timestamp <see cref="string"/>.
        /// </summary>
        /// <returns>The unstyled Discord timestamp <see cref="string"/>.</returns>
        public string AsUnstyled() =>
            $"<t:{Value.ToUnixTimeSeconds()}>";

        /// <summary>
        /// Gets the styled Discord timestamp <see cref="string"/>.
        /// </summary>
        /// <param name="style">The style identifier.</param>
        /// <returns>The styled Discord timestamp <see cref="string"/>.</returns>
        ///
        /// <remarks>The valid style identifiers are available at <see href="https://github.com/discord/discord-api-docs/blob/ff4d9d8ea6493405a8823492338880c47fb02996/docs/Reference.md#timestamp-styles"/>.</remarks>
        private string AsStyled(char style) =>
            $"<t:{Value.ToUnixTimeSeconds()}:{style}>";

        /// <summary>
        /// Gets the styled Discord timestamp <see cref="string"/>.
        ///
        /// Styled as "Short Time" (e.g. 16:20).
        /// </summary>
        /// <returns>The styled Discord timestamp <see cref="string"/> styled as "Short Time".</returns>
        public string AsShortTime() =>
            AsStyled('t');

        /// <summary>
        /// Gets the styled Discord timestamp <see cref="string"/>.
        ///
        /// Styled as "Long Time" (e.g. 16:20:30).
        /// </summary>
        /// <returns>The styled Discord timestamp <see cref="string"/> styled as "Long Time".</returns>
        public string AsLongTime() =>
            AsStyled('T');

        /// <summary>
        /// Gets the styled Discord timestamp <see cref="string"/>.
        ///
        /// Styled as "Short Date" (e.g. 20/04/2021).
        /// </summary>
        /// <returns>The styled Discord timestamp <see cref="string"/> styled as "Short Date".</returns>
        public string AsShortDate() =>
            AsStyled('d');

        /// <summary>
        /// Gets the styled Discord timestamp <see cref="string"/>.
        ///
        /// Styled as "Long Date" (e.g. 20 April 2021).
        /// </summary>
        /// <returns>The styled Discord timestamp <see cref="string"/> styled as "Long Date".</returns>
        public string AsLongDate() =>
            AsStyled('D');

        /// <summary>
        /// Gets the styled Discord timestamp <see cref="string"/>.
        ///
        /// Styled as "Short Date/Time" (e.g. 20 April 2021 16:20).
        /// </summary>
        /// <returns>The styled Discord timestamp <see cref="string"/> styled as "Short Date/Time".</returns>
        public string AsShortDateTime() =>
            AsStyled('f');

        /// <summary>
        /// Gets the styled Discord timestamp <see cref="string"/>.
        ///
        /// Styled as "Long Date/Time" (e.g. Tuesday, 20 April 2021 16:20).
        /// </summary>
        /// <returns>The styled Discord timestamp <see cref="string"/> styled as "Long Date/Time".</returns>
        public string AsLongDateTime() =>
            AsStyled('F');

        /// <summary>
        /// Gets the styled Discord timestamp <see cref="string"/>.
        ///
        /// Styled as "Relative Time" (e.g. 2 months ago).
        /// </summary>
        /// <returns>The styled Discord timestamp <see cref="string"/> styled as "Relative Time".</returns>
        public string AsRelativeTime() =>
            AsStyled('R');

        /// <inheritdoc />
        public override string ToString() =>
            AsShortDate(); // this is the default style according to Discord API docs;

        /// <inheritdoc />
        public override bool Equals(object? obj) =>
            obj switch
            {
                DiscordTimestamp other => Equals(other),
                _ => false
            };

        /// <inheritdoc />
        public bool Equals(DiscordTimestamp other) =>
            MemoryExtensions.Equals(Value, other.Value);

        /// <inheritdoc />
        public override int GetHashCode() =>
            Value.GetHashCode();
        
        public static bool operator ==(DiscordTimestamp left, DiscordTimestamp right)
            => left.Equals(right);
        
        public static bool operator !=(DiscordTimestamp left, DiscordTimestamp right)
            => !left.Equals(right);
    }
}