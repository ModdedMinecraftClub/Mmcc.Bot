using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Common.Extensions.Database.Entities;
using Mmcc.Bot.Common.Extensions.Remora.Discord.API.Abstractions.Rest;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Models.Settings;
using Mmcc.Bot.Common.Statics;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.RemoraAbstractions.Conditions.Attributes;
using Mmcc.Bot.RemoraAbstractions.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Rest.Core;
using Remora.Results;

namespace Mmcc.Bot.Commands.Moderation.MemberApplications;

/// <summary>
/// Commands for managing member applications.
/// </summary>
[Group("apps")]
[Description("Member applications")]
[RequireGuild]
public class MemberApplicationsCommands : CommandGroup
{
    private readonly MessageContext _context;
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly IMediator _mediator;
    private readonly IColourPalette _colourPalette;
    private readonly DiscordSettings _discordSettings;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly ICommandResponder _responder;

    /// <summary>
    /// Instantiates a new instance of <see cref="MemberApplicationsCommands"/>.
    /// </summary>
    /// <param name="context">The message context.</param>
    /// <param name="channelApi">The channel API.</param>
    /// <param name="mediator">The mediator.</param>
    /// <param name="colourPalette">The colour palette.</param>
    /// <param name="discordSettings">The Discord settings.</param>
    /// <param name="guildApi">The guild API.</param>
    /// <param name="responder">The command responder.</param>
    public MemberApplicationsCommands(
        MessageContext context,
        IDiscordRestChannelAPI channelApi,
        IMediator mediator,
        IColourPalette colourPalette,
        DiscordSettings discordSettings,
        IDiscordRestGuildAPI guildApi,
        ICommandResponder responder
    )
    {
        _context = context;
        _channelApi = channelApi;
        _mediator = mediator;
        _colourPalette = colourPalette;
        _discordSettings = discordSettings;
        _guildApi = guildApi;
        _responder = responder;
    }

    [Command("info")]
    [Description("Gets info about the member role")]
    public async Task<IResult> Info() =>
        await _mediator.Send(new GetInfoData.Query(_context.GuildID.Value)) switch
        {
            { IsSuccess: true, Entity: var (memberAppsChannelId, staffRoleId) } =>
                await _responder.Respond(new Embed
                {
                    Title = "How to obtain the Member role",
                    Description =
                        "When first joining the server, players are given the Guest rank. Member is a rank that can be accessed for free by all players. To apply for the rank, complete the following steps:",
                    Fields = new List<EmbedField>
                    {
                        new(":one: Read the requirements",
                            "Requirements for the Member role are available on our **[wiki](https://wiki.moddedminecraft.club/index.php?title=How_to_earn_the_Member_rank)**.",
                            false),
                        new(":two: Read the application format",
                            "When applying, to ensure that your application is processed swiftly, please follow the following application message format:\n" +
                            "```IGN: john01dav\nServer: Enigmatica 2: Expert```\n" +
                            "In addition to the above, please include a screenshot of the required setup with your message. The screenshot should be sent directly via Discord. Do not link a screenshot uploaded to a 3rd party service like gyazo or imgur. Both the information (in the required format), as well as the screenshot should be sent as a single Discord message, not as two separate messages.",
                            false),
                        new(":three: Apply",
                            $"After you've familiarized yourself with the requirements and are reasonably sure you meet them, head over to <#{memberAppsChannelId}> and apply! Remember about the correct format :wink:."),
                        new(":four: Wait for reply",
                            $"As soon as you post your application, the bot will let you know that it has been submitted (if it doesn't then most likely you didn't adhere to the application format). Now all you have to do is wait for a <@&{staffRoleId}> member to process your application. You will be pinged by the bot once it has been processed. You can track your application via this bot's commands. To obtain the ID do `!apps pending`. You can then view its status at any time by doing `!apps view <applicationId>`. You can see other available commands by doing `!help`.\n\n" +
                            $"*We try to process applications as quickly as possible. If you feel like your application has been missed (defined as pending for over 48h), please ping a <@&{staffRoleId}> member.*",
                            false)
                    },
                    Thumbnail = EmbedProperties.MmccLogoThumbnail,
                    Timestamp = DateTimeOffset.UtcNow,
                    Colour = _colourPalette.Blue
                }),

            { IsSuccess: false } res => res
        };

    /// <summary>
    /// Views a member application by ID.
    /// </summary>
    /// <param name="id">ID of the application.</param>
    /// <returns>Result of the operation.</returns>
    [Command("view", "v")]
    [Description("Views a member application by ID.")]
    public async Task<IResult> View(int id) =>
        await _mediator.Send(new GetById.Query
            {
                ApplicationId = id,
                GuildId = _context.GuildID.Value
            }) switch
            {
                { IsSuccess: true, Entity: { } e } =>
                    await _responder.Respond(e.GetEmbed(_colourPalette)),

                { IsSuccess: true } =>
                    Result.FromError(new NotFoundError($"Application with ID `{id}` could not be found.")),

                { IsSuccess: false } res => res
            };

    /// <summary>
    /// Views the next pending application in the queue.
    /// </summary>
    /// <returns>Result of the operation.</returns>
    [Command("next", "n")]
    [Description("Views the next pending application in the queue")]
    public async Task<IResult> ViewNextPending() =>
        await _mediator.Send(new GetNextPending.Query { GuildId = _context.GuildID.Value }) switch
        {
            { IsSuccess: true, Entity: { } e } =>
                await _responder.Respond(e.GetEmbed(_colourPalette)),

            { IsSuccess: true } =>
                await _responder.Respond(new Embed
                {
                    Title = "No pending applications",
                    Description = "There are no pending applications at the moment",
                    Thumbnail = EmbedProperties.MmccLogoThumbnail,
                    Colour = _colourPalette.Blue
                }),

            { IsSuccess: false } res => res
        };

    /// <summary>
    /// Views pending applications.
    /// </summary>
    /// <returns>Result of the operation.</returns>
    [Command("pending", "p")]
    [Description("Views pending applications.")]
    public async Task<IResult> ViewPending()
    {
        var embedBase = new Embed
        {
            Title = "Pending applications",
            Thumbnail = EmbedProperties.MmccLogoThumbnail,
            Colour = _colourPalette.Blue
        };

        return await _mediator.Send(new GetByStatus.Query
            {
                GuildId = _context.GuildID.Value,
                ApplicationStatus = ApplicationStatus.Pending,
                Limit = 25,
                SortByDescending = false
            }) switch
            {
                { IsSuccess: true, Entity: { } e } =>
                    await _responder.Respond(
                        !e.Any()
                            ? embedBase with { Description = "There are no pending applications at the moment." }
                            : embedBase with { Fields = e.GetEmbedFields().ToList() }
                    ),

                { IsSuccess: false } res => res
            };
    }
        
    /// <summary>
    /// Views last 10 approved applications.
    /// </summary>
    /// <returns>Result of the operation.</returns>
    [Command("approved")]
    [Description("Views last 10 approved applications.")]
    public async Task<IResult> ViewApproved()
    {
        var embedBase = new Embed
        {
            Title = "Approved applications",
            Thumbnail = EmbedProperties.MmccLogoThumbnail,
            Colour = _colourPalette.Green
        };

        return await _mediator.Send(new GetByStatus.Query
            {
                GuildId = _context.GuildID.Value,
                ApplicationStatus = ApplicationStatus.Approved,
                Limit = 10,
                SortByDescending = true
            }) switch
            {
                { IsSuccess: true, Entity: { } e } =>
                    await _responder.Respond(
                        !e.Any()
                            ? embedBase with { Description = "You have not approved any applications yet." }
                            : embedBase with { Fields = e.GetEmbedFields().ToList() }
                    ),

                { IsSuccess: false } res => res
            };
    }
        
    /// <summary>
    /// Views last 10 rejected applications.
    /// </summary>
    /// <returns>Result of the operation</returns>
    [Command("rejected")]
    [Description("Views last 10 rejected applications.")]
    public async Task<IResult> ViewRejected()
    {
        var embedBase = new Embed
        {
            Title = "Rejected applications",
            Thumbnail = EmbedProperties.MmccLogoThumbnail,
            Colour = _colourPalette.Red
        };

        return await _mediator.Send(new GetByStatus.Query
            {
                GuildId = _context.GuildID.Value,
                ApplicationStatus = ApplicationStatus.Rejected,
                Limit = 10,
                SortByDescending = true
            }) switch
            {
                { IsSuccess: true, Entity: { } e } =>
                    await _responder.Respond(
                        !e.Any()
                            ? embedBase with { Description = "You have not rejected any applications yet." }
                            : embedBase with { Fields = e.GetEmbedFields().ToList() }
                    ),

                { IsSuccess: false } res => res
            };
    }

    /// <summary>
    /// Approves a member application.
    /// </summary>
    /// <param name="id">ID of the application to approve.</param>
    /// <param name="serverPrefix">Server prefix.</param>
    /// <param name="ignsList">IGN(s) of the player(s).</param>
    /// <returns>The result of the operation.</returns>
    [Command("approve", "a")]
    [Description("Approves a member application.")]
    [RequireUserGuildPermission(DiscordPermission.BanMembers)]
    public async Task<IResult> Approve(int id, string serverPrefix, List<string> ignsList)
    {
        var getMembersChannelResult = await _guildApi.FindGuildChannelByName(_context.GuildID.Value,
            _discordSettings.ChannelNames.MemberApps);
        if (!getMembersChannelResult.IsSuccess)
        {
            return getMembersChannelResult;
        }

        var commandResult = await _mediator.Send(new ApproveAutomatically.Command
        {
            Id = id,
            GuildId = _context.GuildID.Value,
            ChannelId = _context.ChannelID,
            ServerPrefix = serverPrefix,
            Igns = ignsList
        });
        if (!commandResult.IsSuccess)
        {
            return commandResult;
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
            return sendUserNotificationEmbedResult;
        }

        var embed = new Embed
        {
            Title = ":white_check_mark: Approved the application successfully",
            Description = $"Application with ID `{id}` has been :white_check_mark: *approved*.",
            Thumbnail = EmbedProperties.MmccLogoThumbnail,
            Colour = _colourPalette.Green
        };
        return await _responder.Respond(embed);
    }
        
    /// <summary>
    /// Rejects a member application.
    /// </summary>
    /// <param name="id">ID of the application to reject.</param>
    /// <param name="reason">Reason for rejection.</param>
    /// <returns>The result of the operation.</returns>
    [Command("reject", "r")]
    [Description("Rejects a member application.")]
    [RequireUserGuildPermission(DiscordPermission.BanMembers)]
    public async Task<IResult> Reject(int id, [Greedy] string reason)
    {
        var getMembersChannelResult = await _guildApi.FindGuildChannelByName(_context.GuildID.Value,
            _discordSettings.ChannelNames.MemberApps);
        if (!getMembersChannelResult.IsSuccess)
        {
            return getMembersChannelResult;
        }

        var rejectCommandResult = await _mediator.Send(new Reject.Command
            {Id = id, GuildId = _context.GuildID.Value});
        if (!rejectCommandResult.IsSuccess)
        {
            return rejectCommandResult;
        }

        var userNotificationEmbed = new Embed
        {
            Title = ":no_entry: Application rejected.",
            Description = "Unfortunately, your application has been rejected.",
            Thumbnail = EmbedProperties.MmccLogoThumbnail,
            Colour = _colourPalette.Red,
            Fields = new List<EmbedField>
            {
                new("Reason", reason, false),
                new("Rejected by", $"<@{_context.User.ID}>", false)
            }
        };
        var sendUserNotificationEmbedResult = await _channelApi.CreateMessageAsync(
            channelID: getMembersChannelResult.Entity.ID,
            embeds: new[] { userNotificationEmbed },
            messageReference: new MessageReference(new Snowflake(rejectCommandResult.Entity.MessageId)));
            
        if (!sendUserNotificationEmbedResult.IsSuccess)
        {
            return sendUserNotificationEmbedResult;
        }

        var staffNotificationEmbed = new Embed
        {
            Title = ":white_check_mark: Rejected the application successfully!",
            Description = $"Application with ID `{id}` has been *rejected*.",
            Thumbnail = EmbedProperties.MmccLogoThumbnail,
            Colour = _colourPalette.Green
        };
        return await _responder.Respond(staffNotificationEmbed);
    }
}