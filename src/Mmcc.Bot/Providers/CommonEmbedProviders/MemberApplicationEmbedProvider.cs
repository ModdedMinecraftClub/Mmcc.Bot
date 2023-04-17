using System;
using System.Collections.Generic;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.CommonEmbedProviders;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.RemoraAbstractions.Timestamps;
using Remora.Discord.API.Objects;

namespace Mmcc.Bot.Providers.CommonEmbedProviders;

public class MemberApplicationEmbedProvider : ICommonEmbedProvider<MemberApplication>
{
    private readonly IColourPalette _colourPalette;

    public MemberApplicationEmbedProvider(IColourPalette colourPalette)
        => _colourPalette = colourPalette;

    public Embed GetEmbed(MemberApplication memberApplication)
    {
        var statusStr = memberApplication.AppStatus.ToString();
        var embedConditionalAttributes = memberApplication.AppStatus switch
        {
            ApplicationStatus.Pending => new
            {
                Colour = _colourPalette.Blue,
                StatusFieldValue = $":clock1: {statusStr}"
            },
            ApplicationStatus.Approved => new
            {
                Colour = _colourPalette.Green,
                StatusFieldValue = $":white_check_mark: {statusStr}"
            },
            ApplicationStatus.Rejected => new
            {
                Colour = _colourPalette.Red,
                StatusFieldValue = $":no_entry: {statusStr}"
            },
            _ => throw new ArgumentOutOfRangeException(nameof(memberApplication))
        };
        
        return new Embed
        {
            Title = $"Member Application #{memberApplication.MemberApplicationId}",
            Description =
                $"Submitted at {new DiscordTimestamp(memberApplication.AppTime).AsStyled(DiscordTimestampStyle.ShortDateTime)}.",
            Fields = new List<EmbedField>
            {
                new
                (
                    "Author",
                    $"{memberApplication.AuthorDiscordName} (ID: `{memberApplication.AuthorDiscordId}`)",
                    false
                ),
                new("Status", embedConditionalAttributes.StatusFieldValue, false),
                new
                (
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
}