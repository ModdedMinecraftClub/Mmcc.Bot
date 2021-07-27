using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mmcc.Bot.Common.Extensions.Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.Common.Models.Settings;
using Mmcc.Bot.Database.Settings;
using Mmcc.Bot.Polychat.Models.Settings;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Gateway;

namespace Mmcc.Bot
{
    /// <summary>
    /// Extension methods to register bot config with the service collection.
    /// </summary>
    public static class ConfigurationSetup
    {
        /// <summary>
        /// Registers bot config with the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="hostContext">The <see cref="HostBuilderContext"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddBotConfig(
            this IServiceCollection services,
            HostBuilderContext hostContext
        )
        {
            services.AddConfigWithValidation<MySqlSettings, MySqlSettingsValidator>(
                hostContext.Configuration.GetSection("MySql"));
            services.AddConfigWithValidation<DiscordSettings, DiscordSettingsValidator>(
                hostContext.Configuration.GetSection("Discord"));
            services.AddConfigWithValidation<PolychatSettings, PolychatSettingsValidator>(
                hostContext.Configuration.GetSection("Polychat"));

            services.Configure<DiscordGatewayClientOptions>(options =>
            {
                options.Intents =
                    GatewayIntents.Guilds
                    | GatewayIntents.DirectMessages
                    | GatewayIntents.GuildMembers
                    | GatewayIntents.GuildBans
                    | GatewayIntents.GuildMessages
                    | GatewayIntents.GuildMessageReactions;
            });

            return services;
        }
    }
}