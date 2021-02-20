using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Statics;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.Infrastructure.Commands.MemberApplications;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Mmcc.Bot.Infrastructure.Queries.Discord;
using Mmcc.Bot.Infrastructure.Queries.MemberApplications;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups
{
    /// <summary>
    /// Commands for managing member applications.
    /// </summary>
    [Group("apps")]
    public class MemberApplicationsCommands : CommandGroup
    {
        private readonly ICommandContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IMediator _mediator;
        private readonly ColourPalette _colourPalette;

        /// <summary>
        /// Instantiates a new instance of <see cref="MemberApplicationsCommands"/>.
        /// </summary>
        /// <param name="context">The command context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="colourPalette">The colour palette.</param>
        public MemberApplicationsCommands(
            ICommandContext context,
            IDiscordRestChannelAPI channelApi,
            IMediator mediator,
            ColourPalette colourPalette
        )
        {
            _context = context;
            _channelApi = channelApi;
            _mediator = mediator;
            _colourPalette = colourPalette;
        }

        /// <summary>
        /// Views a member application by ID.
        /// </summary>
        /// <param name="id">ID of the application.</param>
        /// <returns>Result of the operation.</returns>
        [Command("view")]
        public async Task<IResult> View(int id)
        {
            if (id < 0)
            {
                return Result.FromError(
                    new ValidationError("Parameter `id` cannot be less than 0.")
                );
            }

            var query = await _mediator.Send(new GetById.Query {ApplicationId = id});
            if (!query.IsSuccess)
            {
                return Result.FromError(query.Error);
            }

            if (query.Entity is null)
            {
                return Result.FromError(
                    new NotFoundError($"Application with ID `{id}` could not be found.")
                );
            }

            var app = query.Entity;
            var embedConditionalAttributes = app.AppStatus switch
            {
                ApplicationStatus.Pending => new
                {
                    Colour = _colourPalette.Blue,
                    StatusFieldValue = ":clock1: PENDING"
                },
                ApplicationStatus.Approved => new
                {
                    Colour = _colourPalette.Green,
                    StatusFieldValue = ":white_check_mark: APPROVED"
                },
                ApplicationStatus.Rejected => new
                {
                    Colour = _colourPalette.Red,
                    StatusFieldValue = ":no_entry: REJECTED"
                },
                _ => throw new ArgumentOutOfRangeException(nameof(id))
            };
            var embed = new Embed
            {
                Title = $"Member Application #{app.MemberApplicationId}",
                Description = $"Submitted at {DateTimeOffset.FromUnixTimeMilliseconds(app.AppTime).UtcDateTime} UTC.",
                Fields = new List<EmbedField>
                {
                    new("Author", $"{app.AuthorDiscordName} (ID: `{app.AuthorDiscordId}`)", false),
                    new("Status", embedConditionalAttributes.StatusFieldValue, false),
                    new(
                        "Provided details",
                        $"{app.MessageContent}\n" +
                        $"**[Original message (click here)](https://discord.com/channels/{app.GuildId}/{app.ChannelId}/{app.MessageId})**",
                        false
                    )
                },
                Colour = embedConditionalAttributes.Colour,
                Thumbnail = EmbedProperties.MmccLogoThumbnail
            };
            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }
        
        /// <summary>
        /// Views pending applications.
        /// </summary>
        /// <returns>Result of the operation.</returns>
        [Command("pending")]
        public async Task<IResult> ViewPending()
        {
            var queryResult = await _mediator.Send(
                new GetByStatus.Query
                {
                    ApplicationStatus = ApplicationStatus.Pending,
                    Limit = 25,
                    SortByDescending = false
                }
            );
            if (!queryResult.IsSuccess)
            {
                return Result.FromError(queryResult.Error);
            }

            var apps = queryResult.Entity;
            var embed = new Embed
            {
                Title = "Pending applications",
                Thumbnail = EmbedProperties.MmccLogoThumbnail,
                Colour = _colourPalette.Blue
            };
            embed = !apps.Any()
                ? embed with {Description = "There are no pending applications at the moment."}
                : embed with {Fields = GetFieldsFromApps(apps).ToList()};
            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }
        
        /// <summary>
        /// Views last 10 approved applications.
        /// </summary>
        /// <returns>Result of the operation.</returns>
        [Command("approved")]
        public async Task<IResult> ViewApproved()
        {
            var queryResult = await _mediator.Send(
                new GetByStatus.Query
                {
                    ApplicationStatus = ApplicationStatus.Approved,
                    Limit = 10,
                    SortByDescending = true
                }
            );
            if (!queryResult.IsSuccess)
            {
                return Result.FromError(queryResult.Error);
            }

            var apps = queryResult.Entity;
            var embed = new Embed
            {
                Title = "Approved applications",
                Thumbnail = EmbedProperties.MmccLogoThumbnail,
                Colour = _colourPalette.Green
            };
            embed = !apps.Any()
                ? embed with {Description = "You have not approved any applications yet."}
                : embed with {Fields = GetFieldsFromApps(apps).ToList()};
            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }
        
        /// <summary>
        /// Views last 10 rejected applications.
        /// </summary>
        /// <returns>Result of the operation</returns>
        [Command("rejected")]
        public async Task<IResult> ViewRejected()
        {
            var queryResult = await _mediator.Send(
                new GetByStatus.Query
                {
                    ApplicationStatus = ApplicationStatus.Rejected,
                    Limit = 10,
                    SortByDescending = true
                }
            );
            if (!queryResult.IsSuccess)
            {
                return Result.FromError(queryResult.Error);
            }

            var apps = queryResult.Entity;
            var embed = new Embed
            {
                Title = "Rejected applications",
                Thumbnail = EmbedProperties.MmccLogoThumbnail,
                Colour = _colourPalette.Red
            };
            embed = !apps.Any()
                ? embed with {Description = "You have not rejected any applications yet."}
                : embed with {Fields = GetFieldsFromApps(apps).ToList()};
            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }

        /// <summary>
        /// Approves a member application.
        /// </summary>
        /// <param name="id">ID of the application to approve.</param>
        /// <param name="serverPrefix">Server prefix.</param>
        /// <param name="ignsList">IGN(s) of the player(s).</param>
        /// <returns>The result of the operation.</returns>
        [Command("approve")]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> Approve(int id, string serverPrefix, List<string> ignsList)
        {
            if (id < 0)
            {
                return Result.FromError(
                    new ValidationError("Parameter `id` cannot be less than 0.")
                );
            }
            if (string.IsNullOrWhiteSpace(serverPrefix))
            {
                return Result.FromError(
                    new ValidationError("Parameter `serverPrefix` cannot be null, empty or whitespace.")
                );
            }
            if (serverPrefix.Length < 2)
            {
                return Result.FromError(
                    new ValidationError("Parameter `serverPrefix` cannot be shorter than `2` characters.")
                );
            }
            if (!ignsList.Any())
            {
                return Result.FromError(
                    new ValidationError("Parameter `ignsList` cannot be empty.")
                );
            }

            var commandResult = await _mediator.Send(new ApproveAutomatically.Command
            {
                Id = id,
                ChannelId = _context.ChannelID,
                ServerPrefix = serverPrefix,
                Igns = ignsList
            });
            if (!commandResult.IsSuccess)
            {
                return Result.FromError(commandResult.Error);
            }

            var embed = new Embed
            {
                Title = ":white_check_mark: Approved the application successfully",
                Description = $"Application with ID `{id}` has been :white_check_mark: *approved*.",
                Thumbnail = EmbedProperties.MmccLogoThumbnail,
                Colour = _colourPalette.Green
            };
            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
                : Result.FromSuccess();
        }
        
        [Command("reject")]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> Reject(int id, [Greedy] string reason)
        {
            if (id < 0)
            {
                return Result.FromError(
                    new ValidationError("Parameter `id` cannot be less than 0.")
                );
            }
            if (string.IsNullOrWhiteSpace(reason))
            {
                return Result.FromError(
                    new ValidationError("Parameter `serverPrefix` cannot be null, empty or whitespace.")
                );
            }
            if (reason.Length < 4)
            {
                return Result.FromError(
                    new ValidationError("Parameter `reason` cannot be shorter than `4` characters.")
                );
            }

            var getChannelResult = await _channelApi.GetChannelAsync(_context.ChannelID);
            if (!getChannelResult.IsSuccess)
            {
                return Result.FromError(getChannelResult);
            }

            var channel = getChannelResult.Entity;
            var guildId = channel.GuildID;
            if (!guildId.HasValue)
            {
                return Result.FromError(
                    new NotFoundError(
                        "Guild could not be found for this channel"));
            }

            var getMembersChannelResult = await _mediator.Send(new GetMembersChannel.Query {GuildId = guildId.Value});
            if (!getMembersChannelResult.IsSuccess)
            {
                return Result.FromError(getMembersChannelResult.Error);
            }

            var rejectCommandResult = await _mediator.Send(new Reject.Command {Id = id});
            if (!rejectCommandResult.IsSuccess)
            {
                return Result.FromError(rejectCommandResult.Error);
            }

            var userNotificationEmbed = new Embed
            {
                Title = ":no_entry: Application rejected.",
                Description = "Unfortunately, your application has been rejected.",
                Thumbnail = EmbedProperties.MmccLogoThumbnail,
                Colour = _colourPalette.Red,
                Fields = new List<EmbedField>
                {
                    new("Reason", reason, new()),
                    new("Rejected by", $"<@{_context.User.ID}>", new())
                }
            };
            var sendUserNotificationEmbedResult =
                await _channelApi.CreateMessageAsync(getMembersChannelResult.Entity.ID,
                    $"<@{rejectCommandResult.Entity.AuthorDiscordId}>", embed: userNotificationEmbed);
            if (!sendUserNotificationEmbedResult.IsSuccess)
            {
                return Result.FromError(sendUserNotificationEmbedResult);
            }

            var staffNotificationEmbed = new Embed
            {
                Title = ":white_check_mark: Rejected the application successfully!",
                Description = $"Application with ID `{id}` has been *rejected*.",
                Thumbnail = EmbedProperties.MmccLogoThumbnail,
                Colour = _colourPalette.Green
            };
            var sendStaffNotificationEmbedResult =
                await _channelApi.CreateMessageAsync(_context.ChannelID, embed: staffNotificationEmbed);
            return !sendStaffNotificationEmbedResult.IsSuccess
                ? Result.FromError(sendStaffNotificationEmbedResult)
                : Result.FromSuccess();
        }
        
        /// <summary>
        /// Gets embed fields from an enumerable of member applications.
        /// </summary>
        /// <param name="memberApplications">An enumerable of member applications.</param>
        /// <returns>Embed fields.</returns>
        private static IEnumerable<EmbedField> GetFieldsFromApps(IEnumerable<MemberApplication> memberApplications) =>
            memberApplications
                .Select(app => new EmbedField
                (
                    $"[{app.MemberApplicationId}] {app.AuthorDiscordName}",
                    $"*Submitted at:* {DateTimeOffset.FromUnixTimeMilliseconds(app.AppTime).UtcDateTime} UTC.",
                    false
                ));
    }
}