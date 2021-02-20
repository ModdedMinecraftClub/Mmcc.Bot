using Remora.Commands.Conditions;

namespace Mmcc.Bot.Infrastructure.Conditions.Attributes
{
    /// <summary>
    /// Marks a command as requiring to be executed within a guild.
    /// </summary>
    public class RequireGuildAttribute : ConditionAttribute
    {
    }
}