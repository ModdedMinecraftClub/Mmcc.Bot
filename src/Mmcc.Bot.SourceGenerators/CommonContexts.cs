namespace Mmcc.Bot.SourceGenerators;

public sealed class CommonContexts
{
    internal class ClassContext
    {
        public string Namespace { get; set; } = null!;
        public string ClassName { get; set; } = null!;   
    }
    
    internal sealed class PropertyContext
    {
        public string Type { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}