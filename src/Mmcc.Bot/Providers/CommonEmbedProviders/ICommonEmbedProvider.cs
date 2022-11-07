using Remora.Discord.API.Objects;

namespace Mmcc.Bot.CommonEmbedProviders;

public interface ICommonEmbedProvider<in T>
{
    /// <summary>
    /// Gets an embed representation of <see cref="T"/>.
    /// </summary>
    Embed GetEmbed(T obj);
}