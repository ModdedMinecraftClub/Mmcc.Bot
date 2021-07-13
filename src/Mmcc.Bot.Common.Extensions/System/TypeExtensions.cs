using System;

namespace Mmcc.Bot.Common.Extensions.System
{
    /// <summary>
    /// Extensions for <see cref="Type"/>.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Converts to friendly display string.
        /// </summary>
        /// <param name="type">Type to convert.</param>
        /// <returns>Type converted to friendly display string.</returns>
        public static string ToFriendlyDisplayString(this Type type)
        {
            var typeStr = type.ToString();
            var displayString = $"{typeStr[(typeStr.LastIndexOf('.') + 1)..]}";

            if (displayString.Contains('+'))
            {
                displayString = displayString[..displayString.LastIndexOf('+')];
            }

            return displayString;
        }
    }
}