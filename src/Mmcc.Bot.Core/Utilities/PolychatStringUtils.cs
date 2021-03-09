using System.Text.RegularExpressions;

namespace Mmcc.Bot.Core.Utilities
{
    public static class PolychatStringUtils
    {
        public static string SanitiseMcMessage(string message) => Regex.Replace(message, "§.", "");
        
        public static string SanitiseMcId(string id) => Regex.Replace(SanitiseMcMessage(id), "[\\[\\]]", "");

        public static string PrefixMessageWithServerId(string id, string message) => $"[{id}] {message}";
    }
}