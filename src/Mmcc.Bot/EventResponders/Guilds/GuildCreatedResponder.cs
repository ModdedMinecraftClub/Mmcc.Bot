using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mmcc.Bot.Common.Errors;
using Mmcc.Bot.Common.Models.Settings;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Mmcc.Bot.EventResponders.Guilds;

/// <summary>
/// Responds to a <see cref="IGuildCreate"/> event.
/// </summary>
public class GuildCreatedResponder : IResponder<IGuildCreate>
{
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly DiscordSettings _discordSettings;
    private readonly ILogger<GuildCreatedResponder> _logger;

    /// <summary>
    /// Instantiates a new instance of <see cref="GuildCreatedResponder"/>.
    /// </summary>
    /// <param name="guildApi">The guild API.</param>
    /// <param name="discordSettings">The Discord settings.</param>
    /// <param name="logger">The logger.</param>
    public GuildCreatedResponder(
        IDiscordRestGuildAPI guildApi,
        DiscordSettings discordSettings,
        ILogger<GuildCreatedResponder> logger
    )
    {
        _guildApi = guildApi;
        _discordSettings = discordSettings;
        _logger = logger;
    }

    public async Task<Result> RespondAsync(IGuildCreate ev, CancellationToken ct = default)
    {
        _logger.LogInformation($"Setting up guild with ID: \"{ev.ID}\" and Name: \"{ev.Name}\"");
            
        var channels = ev.Channels;
        List<string> requiredChannels = _discordSettings.ChannelNames
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.CanRead && p.PropertyType == typeof(string))
            .Select(p => p.GetValue(_discordSettings.ChannelNames) as string)
            .Where(s => s is not null)
            .ToList()!;


        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var requiredChannel in requiredChannels)
        {
            // ReSharper disable once InvertIf
            if (channels.FirstOrDefault(c => c.Name.HasValue && c.Name.Value!.Equals(requiredChannel)) is null)
            {
                var createChannelResult =
                    await _guildApi.CreateGuildChannelAsync(ev.ID, requiredChannel, ChannelType.GuildText, ct: ct);

                if (!createChannelResult.IsSuccess)
                {
                    return new SetupError("Failed to create required channels.");
                }

                _logger.LogInformation(
                    $"Created required channel \"{requiredChannel}\" in guild with ID: \"{ev.ID}\" and Name: \"{ev.Name}\"");
            }
        }

        _logger.LogInformation($"Successfully set up guild with ID: \"{ev.ID}\" and Name: \"{ev.Name}\"");
        return Result.FromSuccess();
    }
}