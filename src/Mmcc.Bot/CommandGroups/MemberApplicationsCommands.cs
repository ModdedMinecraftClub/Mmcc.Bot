using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Errors;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Statics;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.Infrastructure.Queries.MemberApplications;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
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
        private readonly IDiscordRestUserAPI _userApi;
        private readonly IMediator _mediator;
        private readonly ColourPalette _colourPalette;

        /// <summary>
        /// Instantiates a new instance of <see cref="MemberApplicationsCommands"/>.
        /// </summary>
        /// <param name="context">The command context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="userApi">The user API.</param>
        /// <param name="colourPalette">The colour palette.</param>
        public MemberApplicationsCommands(
            ICommandContext context,
            IDiscordRestChannelAPI channelApi,
            IMediator mediator,
            IDiscordRestUserAPI userApi,
            ColourPalette colourPalette
        )
        {
            _context = context;
            _channelApi = channelApi;
            _mediator = mediator;
            _userApi = userApi;
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
                    StatusFieldValue = ":white_check_mark: REJECTED"
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
                    Limit = 100,
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
            
            if (!apps.Any())
            {
                embed = embed with {Description = "There are no pending applications at the moment."};
            }
            else
            {
                embed = embed with {Fields = GetFieldsFromApps(apps).ToList()};
            }
            
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

            if (!apps.Any())
            {
                embed = embed with {Description = "You have not approved any applications yet."};
            }
            else
            {
                embed = embed with {Fields = GetFieldsFromApps(apps).ToList()};
            }

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

            if (!apps.Any())
            {
                embed = embed with {Description = "You have not rejected any applications yet"};
            }
            else
            {
                embed = embed with {Fields = GetFieldsFromApps(apps).ToList()};
            }
            
            var sendMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            return !sendMessageResult.IsSuccess
                ? Result.FromError(sendMessageResult)
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
                    $"*Submitted at:* {DateTimeOffset.FromUnixTimeMilliseconds(app.AppTime).UtcDateTime}",
                    false
                ));
    }
}