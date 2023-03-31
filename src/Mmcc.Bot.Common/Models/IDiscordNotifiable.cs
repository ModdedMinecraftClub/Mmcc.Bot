using Remora.Rest.Core;

namespace Mmcc.Bot.Common.Models;

public interface IDiscordNotifiable
{
    public Snowflake TargetGuildId { get; }
}