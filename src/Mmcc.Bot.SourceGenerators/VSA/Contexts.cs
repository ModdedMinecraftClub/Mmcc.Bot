namespace Mmcc.Bot.SourceGenerators.VSA;

internal sealed class Contexts
{
    internal class ClassContext
    {
        public string Namespace { get; set; } = null!;
        public string ClassName { get; set; } = null!;   
    }
    
    internal sealed class VSAClassContext : ClassContext
    {
        public RequestClassContext RequestClassContext { get; set; } = null!;
        public DiscordCommandContext DiscordCommandContext { get; set; } = null!;
        public bool ShouldHandleNullReturn { get; set; }
        public IReadOnlyList<string> RemoraConditionsArguments { get; set; } = null!;
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

    internal sealed class DiscordViewContext : ClassContext
    {
        public bool HasOnEmpty { get; set; }
    }

    internal sealed class PropertyContext
    {
        public string Type { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
