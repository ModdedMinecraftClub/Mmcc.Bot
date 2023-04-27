using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Statics;
using Mmcc.Bot.Polychat.MessageSenders;
using Mmcc.Bot.Polychat.Services;
using Mmcc.Bot.RemoraAbstractions.Conditions.CommandSpecific;
using Mmcc.Bot.RemoraAbstractions.Services.MessageResponders;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Mmcc.Bot.Commands.Minecraft;

[Group("mc")]
[Description("Minecraft (Polychat)")]
[RequireGuild]
public class MinecraftServersCommands : CommandGroup
{
    private readonly MessageContext _context;
    private readonly IMediator _mediator;
    private readonly IColourPalette _colourPalette;
    private readonly IPolychatService _polychatService;
    private readonly CommandMessageResponder _responder;
    
    public MinecraftServersCommands(
        MessageContext context,
        IMediator mediator,
        IColourPalette colourPalette,
        IPolychatService polychatService,
        CommandMessageResponder responder
    )
    {
        _context = context;
        _mediator = mediator;
        _colourPalette = colourPalette;
        _polychatService = polychatService;
        _responder = responder;
    }
    
    [Command("tps")]
    [Description("Shows current TPS of a MC server")]
    public async Task<IResult> Tps(string serverId) =>
        await _mediator.Send(new SendTpsCommand.Command(serverId, _context.ChannelID));
    
    [Command("exec", "e", "execute")]
    [Description("Executes a command on a MC server")]
    [RequireUserGuildPermission(DiscordPermission.BanMembers)]
    public async Task<IResult> Exec(string serverId, [Greedy] IEnumerable<string> args) =>
        await _mediator.Send(new SendExecCommand.Command(serverId, _context.ChannelID, args));
    
    [Command("restart", "r")]
    [Description("Restarts a server")]
    [RequireUserGuildPermission(DiscordPermission.BanMembers)]
    public async Task<IResult> Restart(string serverId) =>
        await _mediator.Send(new SendRestartCommand.Command(serverId, _context.ChannelID));
    
    [Command("online", "o")]
    [Description("Shows info about online servers")]
    public async Task<IResult> Online()
    {
        var serversInformation = _polychatService.GetInformationAboutOnlineServers().ToList();
        var totalOnlinePlayers = 0;
        var fields = new List<EmbedField>();

        foreach (var serverInformation in serversInformation)
        {
            totalOnlinePlayers += serverInformation.PlayersOnline;

            var fieldName =
                $"[{serverInformation.ServerId}] {serverInformation.ServerName} [{serverInformation.PlayersOnline}/{serverInformation.MaxPlayers}]";
            var fieldValueSb = new StringBuilder();
                
            fieldValueSb.AppendLine($"*{serverInformation.ServerAddress}*");

            if (serverInformation.OnlinePlayerNames.Any())
            {
                fieldValueSb.AppendLine(string.Join(", ", serverInformation.OnlinePlayerNames));
            }

            fields.Add(new(fieldName, fieldValueSb.ToString(), false));
        }
            
        var description =
            $"**Servers online:** {serversInformation.Count}\n**Total players online:** {totalOnlinePlayers}";
        var embed = new Embed
        {
            Title = "Online servers",
            Description = description,
            Colour = _colourPalette.Green,
            Timestamp = DateTimeOffset.UtcNow,
            Thumbnail = EmbedProperties.MmccLogoThumbnail,
            Fields = fields
        };
        return await _responder.Respond(embed);
    }
}