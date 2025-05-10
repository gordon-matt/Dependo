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
    public IContainerBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            NativeContainer.Register(serviceType, implementationType, GetReuse(lifetime), serviceKey: name);
        }
        else
        {
            NativeContainer.Register(serviceType, implementationType, GetReuse(lifetime));
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
        where TService : class
        where TImplementation : class, TService
    {
        if (!string.IsNullOrEmpty(name))
        {
            NativeContainer.Register<TService, TImplementation>(GetReuse(lifetime), serviceKey: name);
        }
        else
        {
            NativeContainer.Register<TService, TImplementation>(GetReuse(lifetime));
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelf<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
        where TImplementation : class
    {
        if (!string.IsNullOrEmpty(name))
        {
            NativeContainer.Register<TImplementation>(GetReuse(lifetime), serviceKey: name);
        }
        else
        {
            NativeContainer.Register<TImplementation>(GetReuse(lifetime));
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance<TService>(TService instance, string? name = null)
        where TService : class
    {
        if (!string.IsNullOrEmpty(name))
        {
            NativeContainer.RegisterInstance(instance, serviceKey: name);
        }
        else
        {
            NativeContainer.RegisterInstance(instance);
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance(Type serviceType, object instance, string? name = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            NativeContainer.RegisterInstance(serviceType, instance, serviceKey: name);
        }
        else
        {
            NativeContainer.RegisterInstance(serviceType, instance);
        }
        return this;
    }

    /// <summary>
    /// Converts ServiceLifetime to DryIoc IReuse
    /// </summary>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>DryIoc reuse</returns>
    private IReuse GetReuse(ServiceLifetime lifetime) =>
        lifetime switch
        {
            ServiceLifetime.Singleton => Reuse.Singleton,
            ServiceLifetime.Scoped => Reuse.Scoped,
            ServiceLifetime.Transient => Reuse.Transient,
            _ => Reuse.Scoped
        };
}