using System;

namespace Mmcc.Bot.Common.Extensions.System
{
    public static class StringExtensions
    {
        public static string[] SplitByNewLine(this string s) =>
            s.Split(
                new[] {"\r\n", "\r", "\n"},
                StringSplitOptions.None
            );

        public static string DoubleQuotes(this string s) => $"\"{s}\"";
    }
}