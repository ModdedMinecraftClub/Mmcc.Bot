using System.Threading;
using System.Threading.Tasks;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Commands.Conditions;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.RemoraAbstractions.Conditions.CommandSpecific;

/// <summary>
/// Marks a command as requiring the requesting user to have a particular permission within the guild. 
/// </summary>
public class RequireUserGuildPermissionAttribute : ConditionAttribute
{
    /// <summary>
    /// Gets the permission.
    /// </summary>
    public DiscordPermission Permission { get; }
        
    /// <summary>
    /// Initializes a new instance of the <see cref="RequireUserGuildPermissionAttribute"/> class.
    /// </summary>
    /// <param name="permission">The permission.</param>
    public RequireUserGuildPermissionAttribute(DiscordPermission permission)
    {
        Permission = permission;
    }
}

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
    public async ValueTask<Result> CheckAsync(RequireUserGuildPermissionAttribute attribute, CancellationToken ct)
        => await _permissionsService.CheckHasRequiredPermission(
            attribute.Permission,
            _context.ChannelID,
            _context.User, ct
        );
}