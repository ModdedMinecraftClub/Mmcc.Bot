using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Core.Models;
using Mmcc.Bot.Core.Statics;
using Mmcc.Bot.Infrastructure.Conditions.Attributes;
using Mmcc.Bot.Infrastructure.Queries.Diagnostics;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.CommandGroups.Diagnostics
{
    /// <summary>
    /// Diagnostics commands.
    /// </summary>
    [Group("diagnostics")]
    public class DiagnosticsCommands : CommandGroup
    {
        private readonly MessageContext _context;
        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly ColourPalette _colourPalette;
        private readonly IMediator _mediator;

        /// <summary>
        /// Instantiates a new instance of <see cref="DiagnosticsCommands"/>.
        /// </summary>
        /// <param name="context">The message context.</param>
        /// <param name="channelApi">The channel API.</param>
        /// <param name="colourPalette">The colour palette.</param>
        /// <param name="mediator">The mediator.</param>
        public DiagnosticsCommands(
            MessageContext context,
            IDiscordRestChannelAPI channelApi,
            ColourPalette colourPalette,
            IMediator mediator
        )
        {
            _context = context;
            _channelApi = channelApi;
            _colourPalette = colourPalette;
            _mediator = mediator;
        }
        
        /// <summary>
        /// Show status of the bot and APIs it uses.
        /// </summary>
        /// <returns>Result of the operation.</returns>
        [Command("bot")]
        [Description("Show status of the bot and APIs it uses")]
        public async Task<IResult> BotDiagnostics()
        {
            var embed = new Embed
            {
                Title = "Bot diagnostics",
                Description = "Diagnostics running... Please wait.",
                Timestamp = DateTimeOffset.Now
            };
            var createMessageResult = await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
            if (!createMessageResult.IsSuccess)
            {
                return Result.FromError(createMessageResult);
            }

            var resourcesToPing = new Dictionary<string, string>
            {
                {"Discord", "discord.com"},
                {"Mojang API", "api.mojang.com"},
                {"MMCC", "s4.moddedminecraft.club"}
            };

            var fields = new List<EmbedField>
            {
                new("Bot status", ":green_circle: Operational", false)
            };

            foreach (var (name, address) in resourcesToPing)
            {
                var pingResult = await _mediator.Send(new PingNetworkResource.Query {Address = address});
                var fieldVal = !pingResult.IsSuccess || pingResult.Entity.Status != IPStatus.Success
                    ? ":x: Could not reach."
                    : pingResult.Entity.RoundtripTime switch
                    {
                        <= 50 => ":green_circle: ",
                        <= 120 => ":yellow_circle: ",
                        _ => ":red_circle: "
                    } + pingResult.Entity.RoundtripTime + " ms";
                
                fields.Add(new($"{name} Status", fieldVal, false));
            }

            var newEmbed = new Embed
            {
                Title = "Bot diagnostics",
                Description = "Information about the status of the bot and the APIs it uses",
                Fields = fields,
                Timestamp = DateTimeOffset.UtcNow,
                Colour = _colourPalette.Green
            };
            return await _channelApi.EditMessageAsync(_context.ChannelID, createMessageResult.Entity.ID, embed: newEmbed);
        }

        /// <summary>
        /// Shows drives info - including free space.
        /// </summary>
        /// <returns>Result of the operation.</returns>
        [Command("drives")]
        [Description("Shows drives info (including free space)")]
        [RequireGuild]
        [RequireUserGuildPermission(DiscordPermission.BanMembers)]
        public async Task<IResult> DrivesDiagnostics()
        {
            var embedFields = new List<EmbedField>();
            var drives = await _mediator.Send(new GetDrives.Query());
            
            foreach (var d in drives)
            {
                if (d is null) break;

                var fieldValue = new StringBuilder();
                var spaceEmoji = d.PercentageUsed switch
                {
                    <= 65 => ":green_circle:",
                    <= 85 => ":yellow_circle:",
                    _ => ":red_circle:"
                };
                
                fieldValue.AppendLine($"Volume label: {d.Label}");
                fieldValue.AppendLine($"File system: {d.DriveFormat}");
                fieldValue.AppendLine($"Available space: {spaceEmoji} {d.GigabytesFree:0.00} GB ({d.PercentageUsed:0.00}% used)");
                fieldValue.AppendLine($"Total size: {d.GigabytesTotalSize:0.00} GB");
                embedFields.Add(new EmbedField(d.Name, fieldValue.ToString(), false));
            }

            var embed = new Embed
            {
                Title = "Drives diagnostics",
                Colour = _colourPalette.Blue,
                Thumbnail = EmbedProperties.MmccLogoThumbnail,
                Footer = new EmbedFooter("Dedicated server"),
                Timestamp = DateTimeOffset.UtcNow,
                Fields = embedFields
            };
            return await _channelApi.CreateMessageAsync(_context.ChannelID, embed: embed);
        }
    }
}