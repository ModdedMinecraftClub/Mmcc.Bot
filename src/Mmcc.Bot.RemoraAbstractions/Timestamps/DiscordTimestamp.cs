using System;

namespace Mmcc.Bot.RemoraAbstractions.Timestamps;

// TODO: use this everywhere instead of DateTimeOffset.ToString();
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
    /// Creates a new <see cref="DiscordTimestamp"/> with the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value in UNIX milliseconds format.</param>
    public DiscordTimestamp(long value) =>
        Value = DateTimeOffset.FromUnixTimeMilliseconds(value);

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
    /// <param name="style">The style.</param>
    /// <returns>The styled Discord timestamp <see cref="string"/>.</returns>
    public string AsStyled(DiscordTimestampStyle style) =>
        $"<t:{Value.ToUnixTimeSeconds()}:{(char)style}>";

    /// <inheritdoc />
    public override string ToString() =>
        // this is the default style according to Discord API docs;
        AsStyled(DiscordTimestampStyle.ShortDateTime);

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