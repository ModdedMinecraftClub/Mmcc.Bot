using Microsoft.Extensions.DependencyInjection;
using Remora.Discord.Commands.Services;

namespace Mmcc.Bot.Middleware
{
    /// <summary>
    /// Extension methods that register middlewares with the service collection.
    /// </summary>
    public static class MiddlewareSetup
    {
        /// <summary>
        /// Registers middlewares with the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddBotMiddlewares(this IServiceCollection services) =>
            services.AddScoped<IPostExecutionEvent, ErrorNotificationMiddleware>();
    }
}