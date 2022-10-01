using System.Collections.Generic;
using System.Linq;
using Remora.Discord.API.Objects;
using Remora.Discord.Interactivity;

namespace Mmcc.Bot.RemoraAbstractions.UI;

public class FluentCallbackModalBuilder :
    FluentCallbackModalBuilder.ITitleSelectionStage,
    FluentCallbackModalBuilder.IComponentsSelectionStage,
    FluentCallbackModalBuilder.IBuildStage
{
    private readonly string? _customId;

    private string? _title;
    private List<ActionRowComponent>? _components;
    
    private FluentCallbackModalBuilder() {}

    private FluentCallbackModalBuilder(string id)
        => _customId = id;

    public static ITitleSelectionStage WithId(string id)
        => new FluentCallbackModalBuilder(CustomIDHelpers.CreateModalID(id));
    
    public static ITitleSelectionStage WithId(string id, params string[] path)
        => new FluentCallbackModalBuilder(CustomIDHelpers.CreateModalID(id, path));
    
    public IComponentsSelectionStage HasTitle(string title)
    {
        _title = title;
        return this;
    }

    public IBuildStage WithCustomActionRows(params ActionRowComponent[] actionRowComponents)
    {
        _components = actionRowComponents.ToList();
        return this;
    }

    public IBuildStage WithActionRowFromTextInputs(params TextInputComponent[] textInputComponents)
    {
        _components = textInputComponents.Select(x => new ActionRowComponent(new[] {x})).ToList();
        return this;
    }

    public InteractionModalCallbackData Build()
        => new(_customId!, _title!, _components!);

    public interface ITitleSelectionStage
    {
        public IComponentsSelectionStage HasTitle(string title);
    }

    public interface IComponentsSelectionStage
    {
        public IBuildStage WithCustomActionRows(params ActionRowComponent[] actionRowComponents);
        public IBuildStage WithActionRowFromTextInputs(params TextInputComponent[] textInputComponents);
    }

    public interface IBuildStage
    {
        public InteractionModalCallbackData Build();
    }
}