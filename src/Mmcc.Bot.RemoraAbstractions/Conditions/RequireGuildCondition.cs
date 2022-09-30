using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.RemoraAbstractions.Conditions.Attributes;
using Remora.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Conditions;

/// <summary>
/// Checks if the command was executed within a guild before allowing execution.
/// </summary>
public class RequireGuildCondition : ICondition<RequireGuildAttribute>
{
    private readonly MessageContext _context;
        
    /// <summary>
    /// Instantiates a new instance of the <see cref="RequireGuildCondition"/> class.
    /// </summary>
    /// <param name="context">The message context.</param>
    public RequireGuildCondition(MessageContext context)
    {
        _context = context;
    }
        
    /// <inheritdoc />
    public ValueTask<Result> CheckAsync(RequireGuildAttribute attribute, CancellationToken ct)
    {
        var guild = _context.GuildID;
        return new(!guild.HasValue
            ? new InvalidOperationError("Command that requires to be executed within a guild was executed outside of one")
            : Result.FromSuccess());
    }
}