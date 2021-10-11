using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.RemoraAbstractions.Conditions.Attributes;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Conditions;

/// <summary>
/// Checks required Guild permissions before allowing execution.
///
/// <remarks>Fails if the command is executed outside of a Guild. It should be used together with <see cref=""./></remarks>
/// </summary>
public class RequireUserGuildPermissionCondition : ICondition<RequireUserGuildPermissionAttribute>
{
    private readonly MessageContext _context;
    private readonly IDiscordPermissionsService _permissionsService;

    /// <summary>
    /// Instantiates a new instance of the <see cref="RequireUserGuildPermissionCondition"/> class.
    /// </summary>
    /// <param name="context">The message context.</param>
    /// <param name="permissionsService">The permissions service.</param>
    public RequireUserGuildPermissionCondition(MessageContext context, IDiscordPermissionsService permissionsService)
    {
        _context = context;
        _permissionsService = permissionsService;
    }

    /// <inheritdoc />
    public async ValueTask<Result> CheckAsync(RequireUserGuildPermissionAttribute attribute, CancellationToken ct) =>
        await _permissionsService.CheckHasRequiredPermission(
            attribute.Permission,
            _context.ChannelID,
            _context.User, ct
        );
}