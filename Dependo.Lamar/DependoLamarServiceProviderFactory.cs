using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.Lamar;

/// <summary>
/// Dependo implementation of Lamar service provider factory for ASP.NET Core
/// </summary>
public class DependoLamarServiceProviderFactory : IServiceProviderFactory<ServiceRegistry>
{
    private readonly Action<ServiceRegistry> configurationAction;
    private IServiceCollection? services;

    /// <summary>
    /// Initializes a new instance of the <see cref="DependoLamarServiceProviderFactory"/> class.
    /// </summary>
    /// <param name="configurationAction">Action on a <see cref="ServiceRegistry"/> that adds component registrations to the container.</param>
    public DependoLamarServiceProviderFactory(Action<ServiceRegistry>? configurationAction = null)
    {
        this.configurationAction = configurationAction ?? (registry => { });
    }

    /// <summary>
    /// Creates a service registry from an <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>A service registry that can be used to create an <see cref="IServiceProvider"/>.</returns>
    public ServiceRegistry CreateBuilder(IServiceCollection services)
    {
        this.services = services;

        var registry = new ServiceRegistry();
        registry.AddRange(services);
        configurationAction(registry);
        return registry;
    }

    /// <summary>
    /// Creates an <see cref="IServiceProvider"/> from the service registry.
    /// </summary>
    /// <param name="serviceRegistry">The service registry.</param>
    /// <returns>An <see cref="IServiceProvider"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when serviceRegistry is null.</exception>
    public IServiceProvider CreateServiceProvider(ServiceRegistry serviceRegistry)
    {
        if (serviceRegistry == null)
        {
            throw new ArgumentNullException(nameof(serviceRegistry));
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
        var dependoContainer = new LamarDependoContainer();
#pragma warning restore DF0010

        var serviceProvider = dependoContainer.ConfigureServices(serviceRegistry, configuration!);

        // Set dependo container as the singleton instance
        DependoResolver.Create(dependoContainer);

        return serviceProvider;
    }
}