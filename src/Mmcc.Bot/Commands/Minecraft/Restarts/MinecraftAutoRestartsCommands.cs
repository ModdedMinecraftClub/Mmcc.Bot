using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Mmcc.Bot.Common.Models.Colours;
using Mmcc.Bot.Common.Statics;
using Mmcc.Bot.Polychat.Jobs.Recurring.Restarts;
using Mmcc.Bot.RemoraAbstractions.Conditions.Attributes;
using Mmcc.Bot.RemoraAbstractions.Services;
using Mmcc.Bot.RemoraAbstractions.Timestamps;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Objects;
using Remora.Results;

namespace Mmcc.Bot.Commands.Minecraft.Restarts;

[Group("restarts")]
[Description("Commands for managing automatic restarts")]
[RequireGuild]
public class MinecraftAutoRestartsCommands : CommandGroup
{
    private readonly IMediator _mediator;
    private readonly IColourPalette _colourPalette;
    private readonly ICommandResponder _responder;

    public MinecraftAutoRestartsCommands(IMediator mediator,
        IColourPalette colourPalette,
        ICommandResponder responder
    )
    {
        _mediator = mediator;
        _colourPalette = colourPalette;
        _responder = responder;
    }

    [Command("new", "update", "n", "u")]
    [Description("Schedules a new recurring restart or updates an existing one with a new cron expression.")]
    public async Task<IResult> ScheduleOrUpdate(string serverId, string cronExpression)
    {
        var baseSuccessEmbed = new Embed
        {
            Title = "Recurring restart has been scheduled successfully.",
            Thumbnail = EmbedProperties.MmccLogoThumbnail,
            Colour = _colourPalette.Green
        };

        return await _mediator.Send(new ScheduleOrUpdate.Command(serverId, cronExpression)) switch
        {
            {IsSuccess: true, Entity: { } job} =>
                await _responder.Respond(baseSuccessEmbed with
                {
                    Fields = new EmbedField[]
                    {
                        new("Server ID", $"`{serverId}`"),
                        new("Job ID", $"`{job.Id}`"),
                        new("Cron", $"`{job.Cron}`"),
                        new("Next execution time",
                            new DiscordTimestamp((DateTimeOffset) job.NextExecution!).AsStyled(DiscordTimestampStyle
                                .RelativeTime))
                    }
                }),

            {IsSuccess: true} =>
                await _responder.Respond(baseSuccessEmbed with
                {
                    Fields = new EmbedField[] {new("Server ID", $"`{serverId}`")}
                }),

            {IsSuccess: false} res => res
        };
    }

    [Command("stop", "s")]
    [Description("Stops a restart job (i.e. stops all future restarts with that ID)")]
    public async Task<IResult> Stop(string serverId) =>
        await _mediator.Send(new Stop.Command(serverId)) switch
        {
            {IsSuccess: true} =>
                await _responder.Respond(new Embed
                {
                    Title = "Recurring restart has been scheduled successfully.",
                    Thumbnail = EmbedProperties.MmccLogoThumbnail,
                    Colour = _colourPalette.Green,
                    Fields = new EmbedField[]
                    {
                        new("Server ID", $"`{serverId}`")
                    }
                }),
            {IsSuccess: false} res => res
        };

    [Command("scheduled", "view", "s", "v")]
    [Description("Views all scheduled recurring restarts.")]
    public async Task<IResult> Scheduled()
    {
        var res = await _mediator.Send(new GetAllScheduled.Query());

        return res.Count switch
        {
            > 0 => await _responder.Respond(new Embed
            {
                Title = "Scheduled recurring restarts",
                Thumbnail = EmbedProperties.MmccLogoThumbnail,
                Timestamp = DateTimeOffset.UtcNow,
                Colour = _colourPalette.Green,
                Fields = res.Select(r =>
                {
                    var (serverId, recurringJobDto) = r;
                    var discordDate = new DiscordTimestamp((DateTimeOffset) recurringJobDto.NextExecution!);
                    var styledDate = discordDate.AsStyled(DiscordTimestampStyle.RelativeTime);
                    
                    return new EmbedField(
                        $"[{serverId}] {recurringJobDto.Id}",
                        $"Next execution: {styledDate}\nCron: `{recurringJobDto.Cron}`"
                    );
                }).ToList()
            }),

            _ => Result.FromError(new NotFoundError("Could not find any scheduled restarts."))
        };
    }
}