using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dependo.LightInject;

/// <summary>
/// Dependo implementation of LightInject service provider factory for ASP.NET Core
/// </summary>
public class DependoLightInjectServiceProviderFactory : IServiceProviderFactory<ServiceContainer>
{
    private readonly Action<ServiceContainer> configurationAction;
    protected IServiceCollection? services;

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
    public virtual ServiceContainer CreateBuilder(IServiceCollection services)
    {
        this.services = services;

        var containerOptions = ContainerOptions.Default
            .Clone()
            .WithMicrosoftSettings()
            .WithAspNetCoreSettings();

        containerOptions.DefaultServiceSelector = services => services.Last();

        var serviceContainer = new ServiceContainer(containerOptions);
        using var rootScope = serviceContainer.BeginScope();
        serviceContainer.RegisterFrom(new LightInjectCompositionRoot(services));
        configurationAction(serviceContainer);
        return serviceContainer;
    }

    /// <summary>
    /// Creates an <see cref="IServiceProvider"/> from the container.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <returns>An <see cref="IServiceProvider"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when container is null.</exception>
    public virtual IServiceProvider CreateServiceProvider(ServiceContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        if (services == null)
        {
            throw new InvalidOperationException("CreateBuilder must be called before CreateServiceProvider.");
        }

        // Build service provider for configuration access
#pragma warning disable DF0010  // Should not be disposed here.
        var provider = services.BuildServiceProvider();
#pragma warning restore DF0010

        var configuration = provider.GetService<IConfiguration>();

        // Initialize dependo container
#pragma warning disable DF0010 // Should not be disposed here.
        var dependoContainer = new LightInjectDependoContainer();
#pragma warning restore DF0010

        var serviceProvider = dependoContainer.ConfigureServices(container, configuration!);

        // Set dependo container as the singleton instance
#pragma warning disable DF0001 // Should not be disposed here.
        DependoResolver.Create(dependoContainer);
#pragma warning restore DF0001

        return serviceProvider;
    }
}