using Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.Hosting.Moderation;

namespace Mmcc.Bot.Hosting
{
    /// <summary>
    /// Extension methods that register bot hosted services with the service collection.
    /// </summary>
    public static class MiddlewareSetup
    {
        /// <summary>
        /// Registers bot hosted services with the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddBotBackgroundServices(this IServiceCollection services) =>
            services.AddHostedService<ModerationBackgroundService>();
    }
}