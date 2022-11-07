using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Commands.Moderation.MemberApplications;
using Mmcc.Bot.Common.Errors;
using Mmcc.Bot.Common.Extensions.Remora.Discord.API.Abstractions.Rest;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Models.Settings;
using Mmcc.Bot.Common.Statics;
using Mmcc.Bot.InMemoryStore.Stores;
using Mmcc.Bot.RemoraAbstractions.Conditions.InteractionSpecific;
using Mmcc.Bot.RemoraAbstractions.Services;
using Mmcc.Bot.RemoraAbstractions.UI;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Interactivity;
using Remora.Rest.Core;
using Remora.Results;

namespace Mmcc.Bot.Interactions.Moderation.MemberApplications;

public class MemberApplicationsInteractions : InteractionGroup
{
    private readonly InteractionContext _context;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IInteractionHelperService _interactionHelperService;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly IMessageMemberAppContextStore _memberAppContextStore;
    private readonly DiscordSettings _discordSettings;
    private readonly IMediator _mediator;
    private readonly IColourPalette _colourPalette;

    public MemberApplicationsInteractions(
        InteractionContext context,
        IDiscordRestChannelAPI channelApi,
        IInteractionHelperService interactionHelperService,
        IDiscordRestGuildAPI guildApi,
        IMessageMemberAppContextStore memberAppContextStore,
        DiscordSettings discordSettings,
        IMediator mediator,
        IColourPalette colourPalette
    )
    {
        _channelApi = channelApi;
        _interactionHelperService = interactionHelperService;
        _guildApi = guildApi;
        _memberAppContextStore = memberAppContextStore;
        _discordSettings = discordSettings;
        _mediator = mediator;
        _colourPalette = colourPalette;
        _context = context;
    }

    [Button("approve-btn")]
    [SuppressInteractionResponse(true)]
    [InteractionRequireGuild]
    [InteractionRequireUserGuildPermission(DiscordPermission.BanMembers)]
    public async Task<Result> OnApproveButtonPressed()
    {
        var serverPrefixInput = FluentTextInputBuilder
            .WithId("serverPrefix")
            .HasStyle(TextInputStyle.Short)
            .HasLabel("Server Prefix")
            .IsRequired(true)
            .Build();
        
        var ignsListInput = FluentTextInputBuilder
            .WithId("igns")
            .HasStyle(TextInputStyle.Paragraph)
            .HasLabel("IGNs List (use space to separate usernames).")
            .IsRequired(true)
            .Build();

        var modal = FluentCallbackModalBuilder
            .WithId("approve")
            .HasTitle("Approve member application")
            .WithActionRowFromTextInputs(serverPrefixInput, ignsListInput)
            .Build();

        return await _interactionHelperService.RespondWithModal(modal);
    }

    [Modal("approve")]
    [SuppressInteractionResponse(true)]
    [InteractionRequireGuild]
    [InteractionRequireUserGuildPermission(DiscordPermission.BanMembers)]
    public async Task<Result> OnApproveModal(string serverPrefix, string igns)
    {
        var notificationResult = await _interactionHelperService.NotifyDeferredMessageIsComing();
        if (!notificationResult.IsSuccess)
        {
            return notificationResult;
        }

        var ignsList = igns.Split(' ').ToList();

        var approveResult = await ApproveMemberApplication(serverPrefix, ignsList);
        if (!approveResult.IsSuccess)
        {
            return Result.FromError(approveResult);
        }

        var sendSuccessEmbed = await _interactionHelperService.SendFollowup(approveResult.Entity);
        return !sendSuccessEmbed.IsSuccess
            ? Result.FromError(sendSuccessEmbed.Error)
            : Result.FromSuccess();
    }

    private async Task<Result<Embed>> ApproveMemberApplication(string serverPrefix, List<string> ignsList)
    {
        if (_context.Message is not {Value.MessageReference.Value.MessageID.HasValue: true})
        {
            return Result<Embed>.FromError(new PropertyMissingOrNullError(
                "The message containing the button unexpectedly did not reference the original command message"));
        }
        
        var messageReference = _context.Message.Value.MessageReference.Value.MessageID.Value;
        
        var memberAppId = _memberAppContextStore.GetOrDefault(messageReference.Value);
        if (memberAppId is null)
        {
            return Result<Embed>.FromError(new InteractionExpiredError(
                $"Interaction has expired in the {typeof(IMessageMemberAppContextStore)} store. Please run the original command again and press the button in the new response."));
        }
        
        var getMembersChannelResult = await _guildApi.FindGuildChannelByName(_context.GuildID.Value,
            _discordSettings.ChannelNames.MemberApps);
        if (!getMembersChannelResult.IsSuccess)
        {
            return Result<Embed>.FromError(getMembersChannelResult);
        }
        
        var commandResult = await _mediator.Send(new ApproveAutomatically.Command
        {
            Id = memberAppId.Value,
            GuildId = _context.GuildID.Value,
            ChannelId = _context.ChannelID,
            ServerPrefix = serverPrefix,
            Igns = ignsList
        });
        if (!commandResult.IsSuccess)
        {
            return Result<Embed>.FromError(commandResult);
        }
        
        var userNotificationEmbed = new Embed
        {
            Title = ":white_check_mark: Application approved.",
            Description = "Your application has been approved.",
            Thumbnail = EmbedProperties.MmccLogoThumbnail,
            Colour = _colourPalette.Green,
            Fields = new List<EmbedField>
            {
                new("Approved by", $"<@{_context.User.ID}>", false)
            }
        };
        var sendUserNotificationEmbedResult = await _channelApi.CreateMessageAsync(
            channelID: getMembersChannelResult.Entity.ID,
            embeds: new[] { userNotificationEmbed },
            messageReference: new MessageReference(new Snowflake(commandResult.Entity.MessageId)));
            
        if (!sendUserNotificationEmbedResult.IsSuccess)
        {
            return Result<Embed>.FromError(sendUserNotificationEmbedResult);
        }
        
        return new Embed
        {
            Title = ":white_check_mark: Approved the application successfully",
            Description = $"Application with ID `{memberAppId.Value}` has been :white_check_mark: *approved*.",
            Thumbnail = EmbedProperties.MmccLogoThumbnail,
            Colour = _colourPalette.Green
        };
    }
}