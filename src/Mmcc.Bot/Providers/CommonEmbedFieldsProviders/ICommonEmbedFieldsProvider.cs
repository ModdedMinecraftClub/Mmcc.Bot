using System.Collections;
using System.Collections.Generic;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.Providers.CommonEmbedFieldsProviders;

public interface ICommonEmbedFieldsProvider<in T> where T : IEnumerable
{
    /// <summary>
    /// Gets an <see cref="IEnumerable{EmbedField}"/> that represent the <see cref="IEnumerable{T}"/>.
    /// </summary>
    IEnumerable<EmbedField> GetEmbedFields(T objs);
}