using static Mmcc.Bot.SourceGenerators.CommonContexts;

namespace Mmcc.Bot.SourceGenerators.DiscordCommands;

internal sealed class DiscordCommandGeneratorContexts
{
    internal sealed class VsaClassContext : ClassContext
    {
        public RequestClassContext RequestClassContext { get; set; } = null!;
        public DiscordCommandContext DiscordCommandContext { get; set; } = null!;
        public bool ShouldHandleNullReturn { get; set; }
        public IReadOnlyList<ConditionAttributeContext> RemoraConditionsAttributeContexts { get; set; } = null!;
    }

    internal sealed class RequestClassContext : ClassContext
    {
        public IReadOnlyList<PropertyContext> Properties { get; set; } = null!;
    }

    internal sealed class DiscordCommandContext : ClassContext
    {
        public bool IsGreedy { get; set; }
        public string CommandName { get; set; } = null!;
        public string CommandDescription { get; set; } = null!;
        public IReadOnlyList<string> CommandAliases { get; set; } = null!;
        public DiscordViewContext MatchedView { get; set; } = null!;
    }
    
    internal sealed class ConditionAttributeContext : ClassContext
    {
        public List<string>? ArgumentsValues { get; set; }
    }

    internal sealed class DiscordViewContext : ClassContext
    {
        public bool HasOnEmpty { get; set; }
    }
}
