namespace Mmcc.Bot.RemoraAbstractions.Timestamps;

/// <summary>
/// Enumerates the <see cref="DiscordTimestamp"/> styles.
/// </summary>
public enum DiscordTimestampStyle
{
    /// <summary>
    /// Represents Discord's "Short Time" (e.g. 16:20).
    /// </summary>
    ShortTime = 't',
    /// <summary>
    /// Represents Discord's "Long Time" (e.g. 16:20:30).
    /// </summary>
    LongTime = 'T',
    /// <summary>
    /// Represents Discord's "Short Date" (e.g. 20/04/2021).
    /// </summary>
    ShortDate = 'd',
    /// <summary>
    /// Represents Discord's "Long Date" (e.g. 20 April 2021).
    /// </summary>
    LongDate = 'D',
    /// <summary>
    /// Represents Discord's "Short Date/Time" (e.g. 20 April 2021 16:20).
    /// </summary>
    ShortDateTime = 'f',
    /// <summary>
    /// Represents Discord's "Long Date/Time" (e.g. Tuesday, 20 April 2021 16:20).
    /// </summary>
    LongDateTime = 'F',
    /// <summary>
    /// Represents Discord's "Relative Time" (e.g. 2 months ago).
    /// </summary>
    RelativeTime = 'R'
}