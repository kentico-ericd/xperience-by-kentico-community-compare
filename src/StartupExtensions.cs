using Microsoft.Extensions.DependencyInjection;

using XperienceCommunity.Compare.Models;
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
    public static IServiceCollection AddXperienceCompare(this IServiceCollection services, Action<CompareModuleOptions>? configureOptions = null)
    {
        var options = new CompareModuleOptions();
        if (configureOptions is not null)
        {
            configureOptions(options);
        }

        services.AddSingleton(options);
        services.AddSingleton<ICompareHelper, CompareHelper>();
        services.AddSingleton<IComparableDataRetriever, ComparableDataRetriever>();

        return services;
    }
}
