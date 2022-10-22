using System.Threading;
using System.Threading.Tasks;
using Remora.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Conditions.InteractionSpecific;

public class InteractionRequireGuildAttribute : ConditionAttribute
{
}

public class InteractionRequireGuildCondition : ICondition<InteractionRequireGuildAttribute>
{
    private readonly InteractionContext _context;

    public InteractionRequireGuildCondition(InteractionContext context)
        => _context = context;

    public ValueTask<Result> CheckAsync(InteractionRequireGuildAttribute attribute, CancellationToken ct = new CancellationToken())
    {
        var guild = _context.GuildID;
        return new(!guild.HasValue
            ? new InvalidOperationError("Command that requires to be executed within a guild was executed outside of one")
            : Result.FromSuccess());
    }
}