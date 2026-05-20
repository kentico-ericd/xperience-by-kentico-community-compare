using Microsoft.Extensions.DependencyInjection;

using XperienceCommunity.Compare.Services;

namespace XperienceCommunity.Compare;

/// <summary>
/// Contains methods to initialize the module during application startup.
/// </summary>
public static class StartupExtensions
{
    /// <summary>
    /// Registers services required by the module.
    /// </summary>
    public static IServiceCollection AddXperienceCompare(this IServiceCollection services)
    {
        services.AddSingleton<IComparableDataRetriever, ComparableDataRetriever>();

        return services;
    }
}
