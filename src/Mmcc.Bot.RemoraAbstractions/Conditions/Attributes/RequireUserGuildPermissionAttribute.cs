using Remora.Commands.Conditions;
using Remora.Discord.API.Abstractions.Objects;

namespace Mmcc.Bot.RemoraAbstractions.Conditions.Attributes;

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