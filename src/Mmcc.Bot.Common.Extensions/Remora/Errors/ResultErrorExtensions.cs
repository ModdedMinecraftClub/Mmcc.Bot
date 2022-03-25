using System.Collections.Generic;
using Mmcc.Bot.Common.Exceptions;
using Remora.Results;

namespace Mmcc.Bot.Common.Extensions.Remora.Errors;

public static class ResultErrorExtensions
{
    public static RemoraErrorException GetException(this IResultError e) =>
        new($"An unhandled exception occurred due to an error that was explicitly thrown on a Remora.IResultError error.\nError: {e.GetType()}\nError message: {e.Message}");

    public static Dictionary<string, string> ToDictionary(this IResultError e) =>
        new()
        {
            ["ErrorType"] = e.GetType().ToString(),
            ["ErrorMessage"] = e.Message
        };
}