using System.Text.RegularExpressions;

namespace Mmcc.Bot.Core.Utilities
{
    public static class PolychatStringUtils
    {
        public static string SanitiseMcId(string id) => Regex.Replace(Regex.Replace(id, "§.", ""), "[\\[\\]]", "");
    }
}