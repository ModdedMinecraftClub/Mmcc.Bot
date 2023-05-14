namespace Mmcc.Bot.SourceGenerators.DiscordCommands;

internal static class Attributes
{
    public static string GenerateDiscordFromMediatRAttribute =>
        """
        namespace Mmcc.Bot.SourceGenerators.DiscordCommands;
        
        [global::System.CodeDom.Compiler.GeneratedCode("Mmcc.Bot.SourceGenerators", "1.0.0")]
        [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
        public class GenerateDiscordCommandAttribute<TCommandGroup> : global::System.Attribute
            where TCommandGroup : global::Remora.Commands.Groups.CommandGroup
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool IsGreedy { get; set; }
            public string[] Aliases { get; set; }
            
            public GenerateDiscordCommandAttribute(
                string name,
                string description,
                bool isGreedy,
                params string[] aliases
            )
            {
                Name = name;
                Description = description;
                IsGreedy = isGreedy;
                Aliases = aliases;
            }
        }
        """;
}
