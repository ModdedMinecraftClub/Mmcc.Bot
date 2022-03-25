using System;

namespace Mmcc.Bot.Common.Exceptions;

public class RemoraErrorException : Exception
{
    public RemoraErrorException()
    {
    }

    public RemoraErrorException(string message) : base(message)
    {
    }

    public RemoraErrorException(string message, Exception inner) : base(message, inner)
    {
    }
}
