using System.Collections.Generic;

namespace Mmcc.Bot.Common.Models.Settings;

public interface IDiagnosticsSettings
{
    int Timeout { get; }
    IReadOnlyDictionary<string, string> NetworkResourcesToCheck { get; }
}

public class DiagnosticsSettings : IDiagnosticsSettings
{
    public int Timeout { get; } = 120;

    public IReadOnlyDictionary<string, string> NetworkResourcesToCheck { get; } = new Dictionary<string, string>
    {
        ["Discord"] = "discord.com",
        ["Mojang API"] = "api.mojang.com",
        ["MMCC"] = "s4.moddedminecraft.club"
    }.AsReadOnly();
}