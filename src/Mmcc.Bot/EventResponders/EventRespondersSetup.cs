using Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.EventResponders.Buttons;
using Mmcc.Bot.EventResponders.Feedback;
using Mmcc.Bot.EventResponders.Guilds;
using Mmcc.Bot.EventResponders.Moderation.MemberApplications;
using Mmcc.Bot.EventResponders.Users;
using Remora.Discord.Gateway.Extensions;

namespace Mmcc.Bot.EventResponders
{
    /// <summary>
    /// Extension methods that register event responders with the service collection.
    /// </summary>
    public static class EventRespondersSetup
    {
        /// <summary>
        /// Registers event responders with the service collection.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddBotGatewayEventResponders(this IServiceCollection services)
        {
            services.AddResponder<GuildCreatedResponder>();
            services.AddResponder<UserJoinedResponder>();
            services.AddResponder<UserLeftResponder>();
            services.AddResponder<FeedbackPostedResponder>();
            services.AddResponder<FeedbackAddressedResponder>();
            services.AddResponder<MemberApplicationCreatedResponder>();
            services.AddResponder<MemberApplicationUpdatedResponder>();
            services.AddResponder<ButtonInteractionCreateResponder>();

            return services;
        }
    }
}