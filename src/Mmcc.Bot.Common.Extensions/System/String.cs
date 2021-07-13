using System;

namespace Mmcc.Bot.Common.Extensions.System
{
    public static class String
    {
        public static string[] SplitByNewLine(this string s) =>
            s.Split(
                new[] {"\r\n", "\r", "\n"},
                StringSplitOptions.None
            );
    }
}