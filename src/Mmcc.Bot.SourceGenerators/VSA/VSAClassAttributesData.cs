namespace Mmcc.Bot.SourceGenerators.VSA;

public sealed class DiscordCommandAttributeData
{
    public INamedTypeSymbol AssociatedCommandGroup { get; set; } = null!;
    public bool IsGreedy { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string[] Aliases { get; set; } = null!;
}