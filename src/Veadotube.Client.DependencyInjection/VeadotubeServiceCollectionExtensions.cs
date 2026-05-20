using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Veadotube.Client;

/// <summary>DI extensions for <see cref="VeadotubeClient"/>.</summary>
public static class VeadotubeServiceCollectionExtensions
{
    /// <summary>
    /// Registers a singleton <see cref="VeadotubeClient"/> wired against the supplied
    /// <see cref="VeadotubeClientOptions"/>.
    /// </summary>
    public static IServiceCollection AddVeadotubeClient(this IServiceCollection services, Action<VeadotubeClientOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (configure is not null)
        {
            _ = services.Configure(configure);
        }
        services.TryAddSingleton(sp =>
        {
            VeadotubeClientOptions options = sp.GetService<IOptions<VeadotubeClientOptions>>()?.Value ?? new VeadotubeClientOptions();
            ILogger<VeadotubeClient>? logger = sp.GetService<ILogger<VeadotubeClient>>();
            return new VeadotubeClient(options, logger);
        });
        return services;
    }
}
