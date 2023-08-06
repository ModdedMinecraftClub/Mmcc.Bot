namespace Mmcc.Bot.SourceGenerators.DiscordCommands;

internal static class DiscordCommandGeneratorAttributes
{
    internal static string DiscordCommandAttribute =>
        """
        namespace Mmcc.Bot.SourceGenerators.DiscordCommands;
        
        [global::System.CodeDom.Compiler.GeneratedCode("Mmcc.Bot.SourceGenerators", "1.0.0")]
        [global::System.AttributeUsage(global::System.AttributeTargets.Class)]
        public class DiscordCommandAttribute<TCommandGroup> : global::System.Attribute
            where TCommandGroup : global::Remora.Commands.Groups.CommandGroup
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public bool IsGreedy { get; set; }
            public string[] Aliases { get; set; }
            
            public DiscordCommandAttribute(
                string name,
                string description,
                bool isGreedy = false,
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
