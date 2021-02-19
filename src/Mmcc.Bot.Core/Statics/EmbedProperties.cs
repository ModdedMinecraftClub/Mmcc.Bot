using Remora.Discord.API.Objects;
using Remora.Discord.Core;

namespace Mmcc.Bot.Core.Statics
{
    public class EmbedProperties
    {
        public static EmbedThumbnail MmccLogoThumbnail => new(Urls.MmccLogoUrl, new Optional<string>(),
            new Optional<int>(), new Optional<int>());
    }
}