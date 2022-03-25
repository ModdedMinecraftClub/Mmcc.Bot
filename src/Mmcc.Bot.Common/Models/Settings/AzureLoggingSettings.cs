using FluentValidation;

namespace Mmcc.Bot.Common.Models.Settings;

public class AzureLoggingSettings
{
    public bool Enabled { get; set; }
    public string? InstrumentationKey { get; set; }
}

public class AzureLoggingSettingsValidator : AbstractValidator<AzureLoggingSettings>
{
    public AzureLoggingSettingsValidator()
    {
        RuleFor(s => s.Enabled)
            .NotEmpty();

        RuleFor(s => s.InstrumentationKey)
            .NotEmpty().When(s => s.Enabled);
    }
}