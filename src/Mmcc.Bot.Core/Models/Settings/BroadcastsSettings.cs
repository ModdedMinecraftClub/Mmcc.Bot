using System.Collections.Generic;

namespace Mmcc.Bot.Core.Models.Settings
{
    public class BroadcastsSettings
    {
        public string Id { get; set; } = null!;
        public string Prefix { get; set; } = null!;
        public List<string>? BroadcastMessages { get; set; }
    }
}