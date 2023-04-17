using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Rest.Core;

namespace Mmcc.Bot.RemoraAbstractions.UI;

public class FluentTextInputBuilder : 
    FluentTextInputBuilder.ITextInputStyleSelectionStage,
    FluentTextInputBuilder.ILabelSelectionStage,
    FluentTextInputBuilder.IRequiredSelectionStage,
    FluentTextInputBuilder.IBuildStage
{
    private readonly string? _customId;
    
    private TextInputStyle? _textInputStyle;
    private string? _label;
    private bool? _isRequired;

    private Optional<int> _minLength;
    private Optional<int> _maxLength;
    private Optional<string> _startingValue;
    private Optional<string> _placeholderValue;
    
    private FluentTextInputBuilder() {}

    private FluentTextInputBuilder(string id)
        => _customId = id;

    public static ITextInputStyleSelectionStage WithId(string id)
        => new FluentTextInputBuilder(id);
    
    public ILabelSelectionStage HasStyle(TextInputStyle style)
    {
        _textInputStyle = style;
        return this;
    }

    public IRequiredSelectionStage HasLabel(string label)
    {
        _label = label;
        return this;
    }

    public IBuildStage IsRequired(bool isRequired)
    {
        _isRequired = isRequired;
        return this;
    }

    public IBuildStage WithMinimumLength(int minLength)
    {
        _minLength = minLength;
        return this;
    }

    public IBuildStage WithMaximumLength(int maxLength)
    {
        _maxLength = maxLength;
        return this;
    }

    public IBuildStage HasStartingValue(string startingValue)
    {
        _startingValue = startingValue;
        return this;
    }

    public IBuildStage HasPlaceholderValue(string placeholderValue)
    {
        _placeholderValue = placeholderValue;
        return this;
    }

    public TextInputComponent Build()
        => new(_customId!, _textInputStyle!.Value, _label!, _minLength, _maxLength, _isRequired!.Value, _startingValue,
            _placeholderValue);

    public interface ITextInputStyleSelectionStage
    {
        public ILabelSelectionStage HasStyle(TextInputStyle style);
    }

    public interface ILabelSelectionStage
    {
        public IRequiredSelectionStage HasLabel(string label);
    }

    public interface IRequiredSelectionStage
    {
        public IBuildStage IsRequired(bool isRequired);
    }

    public interface IBuildStage
    {
        public IBuildStage WithMinimumLength(int minLength);
        public IBuildStage WithMaximumLength(int maxLength);
        public IBuildStage HasStartingValue(string startingValue);
        public IBuildStage HasPlaceholderValue(string placeholderValue);
        public TextInputComponent Build();
    }
}