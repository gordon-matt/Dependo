using DryIoc;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.DryIoc;

/// <summary>
/// DryIoc implementation of the container builder abstraction
/// </summary>
public class DryIocContainerBuilder : IContainerBuilder
{
    /// <summary>
    /// Initializes a new instance of the DryIocContainerBuilder class
    /// </summary>
    /// <param name="container">DryIoc container</param>
    public DryIocContainerBuilder(IContainer container)
    {
        NativeContainer = container;
    }

    /// <summary>
    /// Gets the native DryIoc container
    /// </summary>
    public IContainer NativeContainer { get; }

    /// <inheritdoc/>
    public IContainerBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        NativeContainer.Register(serviceType, implementationType, GetReuse(lifetime));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        NativeContainer.Register<TService, TImplementation>(GetReuse(lifetime));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelf<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        NativeContainer.Register<TImplementation>(GetReuse(lifetime));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance<TService>(TService instance)
        where TService : class
    {
        NativeContainer.RegisterInstance(instance);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance(Type serviceType, object instance)
    {
        NativeContainer.RegisterInstance(serviceType, instance);
        return this;
    }

    /// <summary>
    /// Converts ServiceLifetime to DryIoc IReuse
    /// </summary>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>DryIoc reuse</returns>
    public static IReuse GetReuse(ServiceLifetime lifetime) =>
        lifetime switch
        {
            ServiceLifetime.Singleton => Reuse.Singleton,
            ServiceLifetime.Scoped => Reuse.Scoped,
            ServiceLifetime.Transient => Reuse.Transient,
            _ => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null)
        };
}