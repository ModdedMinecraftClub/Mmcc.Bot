using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.Commands.Services;
using Remora.Discord.Core;

namespace Mmcc.Bot.Infrastructure.HostedServices
{
    public class SlashUpdatingHostedService : IHostedService
    {
        private readonly SlashService _slashService;
        private readonly ILogger<SlashUpdatingHostedService> _logger;

        public SlashUpdatingHostedService(SlashService slashService, ILogger<SlashUpdatingHostedService> logger)
        {
            _slashService = slashService;
            _logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Snowflake? debugServer = null;
            var debugServerString = Environment.GetEnvironmentVariable("REMORA_DEBUG_SERVER");
            if (debugServerString is not null)
            {
                if (!Snowflake.TryParse(debugServerString, out debugServer))
                {
                    _logger.LogWarning("Failed to parse debug server from environment");
                }
            }

            if (!_slashService.SupportsSlashCommands())
            {
                _logger.LogWarning("The registered commands of the bot don't support slash commands");
            }
            else
            {
                var updateSlash = await _slashService.UpdateSlashCommandsAsync(debugServer, cancellationToken);
                if (!updateSlash.IsSuccess)
                {
                    _logger.LogWarning("Failed to update slash commands: {Reason}", updateSlash.Error.Message);
                }
                else
                {
                    _logger.LogInformation("Updated slash commands.");
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}