using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Mmcc.Bot.CommonEmbedProviders;
using Mmcc.Bot.Database.Entities;
using Mmcc.Bot.Providers.CommonEmbedFieldsProviders;
using Mmcc.Bot.Providers.CommonEmbedProviders;

namespace Mmcc.Bot.Providers;

/// <summary>
/// Extension methods that register Mmcc.Bot.Providers with the service collection.
/// </summary>
public static class ProvidersSetup
{
    /// <summary>
    /// Registers Mmcc.Bot.Providers classes with the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddProviders(this IServiceCollection services)
    {
        services.AddSingleton<ICommonEmbedProvider<MemberApplication>, MemberApplicationEmbedProvider>();

        services
            .AddSingleton
            <
                ICommonEmbedFieldsProvider<IEnumerable<MemberApplication>>,
                MemberApplicationsEmbedFieldProvider
            >();

        return services;
    }
}