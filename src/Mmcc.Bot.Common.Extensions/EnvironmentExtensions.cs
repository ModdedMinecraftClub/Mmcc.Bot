using System;

namespace Mmcc.Bot.Common.Extensions;

public static class EnvironmentExtensions
{
    public static bool IsDocker()
    {
        var isContainerStr = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER");

        if (string.IsNullOrWhiteSpace(isContainerStr))
        {
            return false;
        }

        var isBool = bool.TryParse(isContainerStr, out var isContainer);

        return isBool && isContainer;
    }
}