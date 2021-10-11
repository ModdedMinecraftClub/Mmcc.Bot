using FluentValidation;

namespace Mmcc.Bot.Polychat.Models.Settings;

/// <summary>
/// Settings for communication with Polychat2.
/// </summary>
public class PolychatSettings
{
    /// <summary>
    /// ID of the channel where MC chat will be forwarded to.
    /// </summary>
    public ulong ChatChannelId { get; set; }

    /// <summary>
    /// Settings for broadcasts.
    /// </summary>
    public BroadcastsSettings? BroadcastsSettings { get; set; }
}

public class PolychatSettingsValidator : AbstractValidator<PolychatSettings>
{
    public PolychatSettingsValidator()
    {
        RuleFor(s => s.ChatChannelId)
            .NotEmpty();
    }
}