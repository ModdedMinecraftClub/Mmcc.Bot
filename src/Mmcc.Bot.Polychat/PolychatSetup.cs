using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.Polychat.Hosting;
using Mmcc.Bot.Polychat.Models.Settings;
using Mmcc.Bot.Polychat.Services;
using Remora.Discord.Gateway.Extensions;
using Ssmp;
using Ssmp.Extensions;

namespace Mmcc.Bot.Polychat
{
    /// <summary>
    /// Extension methods that register Mmcc.Bot.Polychat with the IoC container.
    /// </summary>
    public static class PolychatSetup
    {
        /// <summary>
        /// Registers Mmcc.Bot.Polychat classes with the IoC container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="ssmpConfig">The Ssmp config - <see cref="SsmpOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddPolychat(
            this IServiceCollection services,
            IConfigurationSection ssmpConfig
        )
        {
            services.AddValidatorsFromAssemblyContaining<PolychatSettings>();
            
            services.AddScoped<IDiscordSanitiserService, DiscordSanitiserService>();
            services.AddSingleton<IPolychatService, PolychatService>();
            
            services.AddSsmp<SsmpHandler>(ssmpConfig);
            
            services.AddResponder<DiscordChatMessageForwarder>();
            
            services.AddHostedService<BroadcastsHostedService>();

            return services;
        }
    }
}