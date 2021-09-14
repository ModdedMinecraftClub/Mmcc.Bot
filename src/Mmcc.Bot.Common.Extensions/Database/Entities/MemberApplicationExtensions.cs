using System;
using System.Collections.Generic;
using System.Linq;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.RemoraAbstractions.Timestamps;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.Common.Extensions.Database.Entities
{
    /// <summary>
    /// Extensions for <see cref="MemberApplication"/>.
    /// </summary>
    public static class MemberApplicationExtensions
    {
        /// <summary>
        /// Get an embed representation of a member application.
        /// </summary>
        /// <param name="memberApplication">Member application.</param>
        /// <param name="colourPalette">Colour palette for the embed to use.</param>
        /// <returns>Embed representing the member application.</returns>
        public static Embed GetEmbed(this MemberApplication memberApplication, IColourPalette colourPalette)
        {
            var statusStr = memberApplication.AppStatus.ToString();
            var embedConditionalAttributes = memberApplication.AppStatus switch
            {
                ApplicationStatus.Pending => new
                {
                    Colour = colourPalette.Blue,
                    StatusFieldValue = $":clock1: {statusStr}"
                },
                ApplicationStatus.Approved => new
                {
                    Colour = colourPalette.Green,
                    StatusFieldValue = $":white_check_mark: {statusStr}"
                },
                ApplicationStatus.Rejected => new
                {
                    Colour = colourPalette.Red,
                    StatusFieldValue = $":no_entry: {statusStr}"
                },
                _ => throw new ArgumentOutOfRangeException(nameof(memberApplication))
            };
            return new Embed
            {
                Title = $"Member Application #{memberApplication.MemberApplicationId}",
                Description =
                    $"Submitted at {new DiscordTimestamp(memberApplication.AppTime).AsStyled(DiscordTimestampStyle.ShortDateTime)} UTC.",
                Fields = new List<EmbedField>
                {
                    new("Author", $"{memberApplication.AuthorDiscordName} (ID: `{memberApplication.AuthorDiscordId}`)",
                        false),
                    new("Status", embedConditionalAttributes.StatusFieldValue, false),
                    new(
                        "Provided details",
                        $"{memberApplication.MessageContent}\n" +
                        $"**[Original message (click here)](https://discord.com/channels/{memberApplication.GuildId}/{memberApplication.ChannelId}/{memberApplication.MessageId})**",
                        false
                    )
                },
                Colour = embedConditionalAttributes.Colour,
                Thumbnail = new EmbedThumbnail(memberApplication.ImageUrl)
            };
        }

        /// <summary>
        /// Gets an enumerable of formatted embed fields that represent the member applications. 
        /// </summary>
        /// <param name="memberApplications">Enumerable of member applications.</param>
        /// <returns>Enumerable of formatted embed fields that represent the member applications.</returns>
        public static IEnumerable<EmbedField> GetEmbedFields(this IEnumerable<MemberApplication> memberApplications) =>
            memberApplications.Select(app => new EmbedField
            (
                $"[{app.MemberApplicationId}] {app.AuthorDiscordName}",
                $"*Submitted at:* {new DiscordTimestamp(app.AppTime).AsStyled(DiscordTimestampStyle.ShortDateTime)} UTC.",
                false
            ));
    }
}