using LightInject;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.LightInject;

/// <summary>
/// Dependo implementation of LightInject service provider factory for ASP.NET Core
/// </summary>
public class DependoLightInjectServiceProviderFactory : IServiceProviderFactory<ServiceContainer>
{
    private readonly Action<ServiceContainer> configurationAction;
    private IServiceCollection? services;

    /// <summary>
    /// Initializes a new instance of the <see cref="DependoLightInjectServiceProviderFactory"/> class.
    /// </summary>
    /// <param name="configurationAction">Action on a <see cref="IServiceContainer"/> that adds component registrations to the container.</param>
    public DependoLightInjectServiceProviderFactory(Action<IServiceContainer>? configurationAction = null)
    {
        this.configurationAction = configurationAction ?? (_ => { });
    }

    /// <summary>
    /// Creates a container from an <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>A container that can be used to create an <see cref="IServiceProvider"/>.</returns>
    public ServiceContainer CreateBuilder(IServiceCollection services)
    {
        this.services = services;

        var serviceContainer = new ServiceContainer();
        using var rootScope = serviceContainer.BeginScope();

        // TODO
        // RegisterServices(serviceContainer, rootScope, services);

        configurationAction(serviceContainer);
        return serviceContainer;
    }

    /// <summary>
    /// Creates an <see cref="IServiceProvider"/> from the container.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <returns>An <see cref="IServiceProvider"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when container is null.</exception>
    public IServiceProvider CreateServiceProvider(ServiceContainer container)
    {
        if (container == null)
        {
            throw new ArgumentNullException(nameof(container));
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
        var dependoContainer = new LightInjectDependoContainer();
#pragma warning restore DF0010

        var serviceProvider = dependoContainer.ConfigureServices(container, configuration!);

        // Set dependo container as the singleton instance
        DependoResolver.Create(dependoContainer);

        return serviceProvider;
    }
}