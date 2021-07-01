using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mmcc.Bot.Core.Extensions.Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Binds a specified configuration section to the specified <typeparamref name="TConfig"/> type and validates it
        /// against the specified <typeparamref name="TValidator"/> and registers it with the IoC container (the <paramref name="services"/> service collection).
        ///
        /// Throws in the event the validation fails.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configurationSection">The configuration section containing the config to be bound to the specified <typeparamref name="TConfig"/>.</param>
        /// <typeparam name="TConfig">Type to which the <paramref name="configurationSection"/> will be bound.</typeparam>
        /// <typeparam name="TValidator">The validator which the <paramref name="configurationSection"/> will be validated against.</typeparam>
        /// <returns>The service collection with the config singleton registered as <typeparamref name="TConfig"/>.</returns>
        ///
        /// <remarks>
        /// This method will add the config as <typeparamref name="TConfig"/> singleton.
        ///
        /// It will not register it as <see cref="IOptions{TOptions}"/>.
        /// </remarks>
        public static IServiceCollection AddConfigWithValidation<TConfig, TValidator>(
            this IServiceCollection services,
            IConfigurationSection configurationSection
        )
            where TConfig : class, new()
            where TValidator : AbstractValidator<TConfig>, new()
        {
            var config = configurationSection.Get<TConfig>();
            var validator = new TValidator();
            
            validator.ValidateAndThrow(config);
            services.AddSingleton(config);

            return services;
        }
    }
}