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

    // https://github.com/dadhi/DryIoc/blob/master/docs/DryIoc.Docs/OpenGenerics.md
    /// <inheritdoc/>
    public IContainerBuilder RegisterGeneric(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped)
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

    /// <inheritdoc/>
    public IContainerBuilder RegisterNamed(Type serviceType, Type implementationType, string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        NativeContainer.Register(serviceType, implementationType, GetReuse(lifetime), serviceKey: name);

        // Additionally, register the service without a name for compatibility when switching between service providers
        Register(serviceType, implementationType, lifetime);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterNamed<TService, TImplementation>(string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        NativeContainer.Register<TService, TImplementation>(GetReuse(lifetime), serviceKey: name);

        // Additionally, register the service without a name for compatibility when switching between service providers
        Register<TService, TImplementation>(lifetime);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelfNamed<TImplementation>(string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        NativeContainer.Register<TImplementation>(GetReuse(lifetime), serviceKey: name);

        // Additionally, register the service without a name for compatibility when switching between service providers
        RegisterSelf<TImplementation>(lifetime);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceNamed<TService>(TService instance, string name)
        where TService : class
    {
        NativeContainer.RegisterInstance(instance, serviceKey: name);

        // Additionally, register the service without a name for compatibility when switching between service providers
        RegisterInstance(instance);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceNamed(Type serviceType, object instance, string name)
    {
        NativeContainer.RegisterInstance(serviceType, instance, serviceKey: name);

        // Additionally, register the service without a name for compatibility when switching between service providers
        RegisterInstance(serviceType, instance);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterKeyed(Type serviceType, Type implementationType, object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        NativeContainer.Register(serviceType, implementationType, GetReuse(lifetime), serviceKey: key);

        // Additionally, register the service without a name for compatibility when switching between service providers
        Register(serviceType, implementationType, lifetime);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterKeyed<TService, TImplementation>(object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        NativeContainer.Register<TService, TImplementation>(GetReuse(lifetime), serviceKey: key);

        // Additionally, register the service without a name for compatibility when switching between service providers
        Register<TService, TImplementation>(lifetime);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelfKeyed<TImplementation>(object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        NativeContainer.Register<TImplementation>(GetReuse(lifetime), serviceKey: key);

        // Additionally, register the service without a name for compatibility when switching between service providers
        RegisterSelf<TImplementation>(lifetime);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceKeyed<TService>(TService instance, object key)
        where TService : class
    {
        NativeContainer.RegisterInstance(instance, serviceKey: key);

        // Additionally, register the service without a name for compatibility when switching between service providers
        RegisterInstance(instance);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceKeyed(Type serviceType, object instance, object key)
    {
        NativeContainer.RegisterInstance(serviceType, instance, serviceKey: key);

        // Additionally, register the service without a name for compatibility when switching between service providers
        RegisterInstance(serviceType, instance);

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