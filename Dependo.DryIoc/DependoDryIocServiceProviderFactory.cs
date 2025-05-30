using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.DryIoc;

/// <summary>
/// Dependo implementation of DryIoc service provider factory for ASP.NET Core
/// </summary>
public class DependoDryIocServiceProviderFactory : IServiceProviderFactory<IContainer>
{
    private readonly Action<IContainer> _configurationAction;
    protected IServiceCollection? _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="DependoDryIocServiceProviderFactory"/> class.
    /// </summary>
    /// <param name="configurationAction">Action on a <see cref="IContainer"/> that adds component registrations to the container.</param>
    public DependoDryIocServiceProviderFactory(Action<IContainer>? configurationAction = null)
    {
        _configurationAction = configurationAction ?? (_ => { });
    }

    /// <summary>
    /// Creates a container from an <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>A container that can be used to create an <see cref="IServiceProvider"/>.</returns>
    public virtual IContainer CreateBuilder(IServiceCollection services)
    {
        _services = services;

        var container =
            new Container(rules => DryIocAdapter
                .WithMicrosoftDependencyInjectionRules(rules)
                .WithFuncAndLazyWithoutRegistration())
            .WithDependencyInjectionAdapter(services).Container;
        _configurationAction(container);
        return container;
    }

    /// <summary>
    /// Creates an <see cref="IServiceProvider"/> from the container.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <returns>An <see cref="IServiceProvider"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when container is null.</exception>
    public virtual IServiceProvider CreateServiceProvider(IContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);

        if (_services == null)
        {
            throw new InvalidOperationException("CreateBuilder must be called before CreateServiceProvider.");
        }

        // Build service provider for configuration access
#pragma warning disable DF0010  // Should not be disposed here.
        var provider = _services.BuildServiceProvider();
#pragma warning restore DF0010

        var configuration = provider.GetService<IConfiguration>();

        // Initialize dependo container
#pragma warning disable DF0010 // Should not be disposed here.
        var dependoContainer = new DryIocDependoContainer();
#pragma warning restore DF0010

        var serviceProvider = dependoContainer.ConfigureServices(container, configuration!);

        // Set dependo container as the singleton instance
        DependoResolver.Create(dependoContainer);

        return serviceProvider;
    }
}