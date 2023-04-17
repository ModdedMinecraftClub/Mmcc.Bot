using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.RemoraAbstractions.Conditions.CommandSpecific;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Commands.Conditions;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Conditions.InteractionSpecific;

public class InteractionRequireUserGuildPermissionAttribute : ConditionAttribute
{
    /// <summary>
    /// Gets the permission.
    /// </summary>
    public DiscordPermission Permission { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequireUserGuildPermissionAttribute"/> class.
    /// </summary>
    /// <param name="permission">The permission.</param>
    public InteractionRequireUserGuildPermissionAttribute(DiscordPermission permission)
        => Permission = permission;
}

public class InteractionRequireUserGuildPermissionCondition : ICondition<InteractionRequireUserGuildPermissionAttribute>
{
    private readonly InteractionContext _context;
    private readonly IDiscordPermissionsService _permissionsService;


    public InteractionRequireUserGuildPermissionCondition(InteractionContext context, IDiscordPermissionsService permissionsService)
    {
        _context = context;
        _permissionsService = permissionsService;
    }

    public async ValueTask<Result> CheckAsync(InteractionRequireUserGuildPermissionAttribute attribute, CancellationToken ct)
        => await _permissionsService.CheckHasRequiredPermission(
            attribute.Permission,
            _context.ChannelID,
            _context.User, ct
        );
}