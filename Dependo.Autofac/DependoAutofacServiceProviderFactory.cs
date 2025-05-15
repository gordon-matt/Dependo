using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.Autofac;

/// <summary>
/// Dependo implementation of Autofac service provider factory for ASP.NET Core
/// </summary>
public class DependoAutofacServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
{
    private readonly Action<ContainerBuilder> configurationAction;
    protected IServiceCollection? services;

    /// <summary>
    /// Initializes a new instance of the <see cref="DependoAutofacServiceProviderFactory"/> class.
    /// </summary>
    /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the container.</param>
    public DependoAutofacServiceProviderFactory(Action<ContainerBuilder>? configurationAction = null)
    {
        this.configurationAction = configurationAction ?? (builder => { });
    }

    /// <summary>
    /// Creates a container builder from an <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>A container builder that can be used to create an <see cref="IServiceProvider"/>.</returns>
    public virtual ContainerBuilder CreateBuilder(IServiceCollection services)
    {
        this.services = services;

        var builder = new ContainerBuilder();
        builder.Populate(services);
        configurationAction(builder);
        return builder;
    }

    /// <summary>
    /// Creates an <see cref="IServiceProvider"/> from the container builder.
    /// </summary>
    /// <param name="containerBuilder">The container builder.</param>
    /// <returns>An <see cref="IServiceProvider"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when containerBuilder is null.</exception>
    public virtual IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
    {
        if (containerBuilder == null)
        {
            throw new ArgumentNullException(nameof(containerBuilder));
        }

        if (services == null)
        {
            throw new InvalidOperationException("CreateBuilder must be called before CreateServiceProvider.");
        }

        // Build service provider for configuration access

#pragma warning disable DF0010  // Should not be disposed here.
        var provider = services.BuildServiceProvider();
#pragma warning restore DF0010

        var configuration = provider.GetService<IConfiguration>();

        // Initialize dependo container and create service provider

#pragma warning disable DF0010 // Should not be disposed here.
        var dependoContainer = new AutofacDependoContainer();
#pragma warning restore DF0010

        var serviceProvider = dependoContainer.ConfigureServices(containerBuilder, configuration!);

        // Set dependo container as the singleton instance
#pragma warning disable DF0001 // Should not be disposed here.
        DependoResolver.Create(dependoContainer);
#pragma warning restore DF0001

        return serviceProvider;
    }
}