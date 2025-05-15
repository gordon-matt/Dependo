using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.DotNetDefault;

/// <summary>
/// Dependo implementation of .NET Default service provider factory for ASP.NET Core
/// </summary>
public class DependoDotNetDefaultServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
{
    private readonly Action<IServiceCollection> configurationAction;
    protected IServiceCollection? services;

    /// <summary>
    /// Initializes a new instance of the <see cref="DependoDotNetDefaultServiceProviderFactory"/> class.
    /// </summary>
    /// <param name="configurationAction">Action on an <see cref="IServiceCollection"/> that adds component registrations to the container.</param>
    public DependoDotNetDefaultServiceProviderFactory(Action<IServiceCollection>? configurationAction = null)
    {
        this.configurationAction = configurationAction ?? (services => { });
    }

    /// <summary>
    /// Creates a service collection from an <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>A service collection that can be used to create an <see cref="IServiceProvider"/>.</returns>
    public virtual IServiceCollection CreateBuilder(IServiceCollection services)
    {
        this.services = services;
        configurationAction(services);
        return services;
    }

    /// <summary>
    /// Creates an <see cref="IServiceProvider"/> from the service collection.
    /// </summary>
    /// <param name="serviceCollection">The service collection.</param>
    /// <returns>An <see cref="IServiceProvider"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when serviceCollection is null.</exception>
    public virtual IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
    {
        if (serviceCollection == null)
        {
            throw new ArgumentNullException(nameof(serviceCollection));
        }

        if (services == null)
        {
            throw new InvalidOperationException("CreateBuilder must be called before CreateServiceProvider.");
        }

        // Build service provider for configuration access
#pragma warning disable DF0010  // Should not be disposed here.
        var provider = services.BuildServiceProvider();
#pragma warning restore DF0010

        var configuration = provider.GetService<IConfigurationRoot>();

        // Initialize dependo container
#pragma warning disable DF0010 // Should not be disposed here.
        var dependoContainer = new DotNetDefaultDependoContainer();
#pragma warning restore DF0010

        var serviceProvider = dependoContainer.ConfigureServices(serviceCollection, configuration!);

        // Set dependo container as the singleton instance
        DependoResolver.Create(dependoContainer);

        return serviceProvider;
    }
} 