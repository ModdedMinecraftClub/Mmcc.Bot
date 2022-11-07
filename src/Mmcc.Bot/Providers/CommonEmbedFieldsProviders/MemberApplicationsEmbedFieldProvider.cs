using System.Collections.Generic;
using System.Linq;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.RemoraAbstractions.Timestamps;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.Providers.CommonEmbedFieldsProviders;

public class MemberApplicationsEmbedFieldProvider : ICommonEmbedFieldsProvider<IEnumerable<MemberApplication>>
{
    public IEnumerable<EmbedField> GetEmbedFields(IEnumerable<MemberApplication> memberApplications)
    {
        return memberApplications.Select(app => new EmbedField
        (
            $"[{app.MemberApplicationId}] {app.AuthorDiscordName}",
            $"*Submitted at:* {new DiscordTimestamp(app.AppTime).AsStyled(DiscordTimestampStyle.ShortDateTime)}.",
            false
        ));
    }
}