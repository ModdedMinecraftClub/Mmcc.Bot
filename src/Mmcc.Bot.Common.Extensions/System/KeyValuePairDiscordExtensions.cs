using System.Collections.Generic;
using System.Linq;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.Common.Extensions.System;

public static class KeyValuePairDiscordExtensions
{
    public static IEnumerable<IEmbedField> ToEmbedFields(this IEnumerable<KeyValuePair<string, string>> kvEnumerable)
        => kvEnumerable.Select(x => new EmbedField(x.Key, x.Value, false));

    public static EmbedField ToEmbedField(this KeyValuePair<string, string> kv)
        => new(kv.Key, kv.Value, false);
}