using Lamar;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.Lamar;

/// <summary>
/// Lamar implementation of the container builder abstraction
/// </summary>
/// <remarks>
/// This is a stub class to demonstrate the concept.
/// In a real implementation, you would use actual Lamar types.
/// </remarks>
public class LamarContainerBuilder : IContainerBuilder
{
    /// <summary>
    /// Initializes a new instance of the LamarContainerBuilder class
    /// </summary>
    /// <param name="registry">Lamar service registry</param>
    public LamarContainerBuilder(ServiceRegistry registry)
    {
        NativeRegistry = registry;
    }

    /// <summary>
    /// Gets the native Lamar service registry
    /// </summary>
    public ServiceRegistry NativeRegistry { get; }

    /// <inheritdoc/>
    public IContainerBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var registration = NativeRegistry.For(serviceType).Use(implementationType);
        registration.Lifetime = GetLifetime(lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        var registration = NativeRegistry.For<TService>().Use<TImplementation>();
        registration.Lifetime = GetLifetime(lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelf<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        var registration = NativeRegistry.For<TImplementation>().Use<TImplementation>();
        registration.Lifetime = GetLifetime(lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance<TService>(TService instance)
        where TService : class
    {
        NativeRegistry.For<TService>().Use(instance);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance(Type serviceType, object instance)
    {
        NativeRegistry.For(serviceType).Use(instance.GetType());
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterNamed(Type serviceType, Type implementationType, string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var registration = NativeRegistry.For(serviceType).Use(implementationType);
        registration.Lifetime = GetLifetime(lifetime);
        registration.Named(name);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterNamed<TService, TImplementation>(string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        var registration = NativeRegistry.For<TService>().Use<TImplementation>();
        registration.Lifetime = GetLifetime(lifetime);
        registration.Named(name);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelfNamed<TImplementation>(string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        var registration = NativeRegistry.For<TImplementation>().Use<TImplementation>();
        registration.Lifetime = GetLifetime(lifetime);
        registration.Named(name);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceNamed<TService>(TService instance, string name)
        where TService : class
    {
        var registration = NativeRegistry.For<TService>().Use(instance);
        registration.Named(name);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceNamed(Type serviceType, object instance, string name)
    {
        var registration = NativeRegistry.For(serviceType).Use(instance.GetType());
        registration.Named(name);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterKeyed(Type serviceType, Type implementationType, object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var registration = NativeRegistry.For(serviceType).Use(implementationType);
        registration.Lifetime = GetLifetime(lifetime);
        registration.Named(DependoHelper.StringifyKey(key));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterKeyed<TService, TImplementation>(object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        var registration = NativeRegistry.For<TService>().Use<TImplementation>();
        registration.Lifetime = GetLifetime(lifetime);
        registration.Named(DependoHelper.StringifyKey(key));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelfKeyed<TImplementation>(object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        var registration = NativeRegistry.For<TImplementation>().Use<TImplementation>();
        registration.Lifetime = GetLifetime(lifetime);
        registration.Named(DependoHelper.StringifyKey(key));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceKeyed<TService>(TService instance, object key)
        where TService : class
    {
        var registration = NativeRegistry.For<TService>().Use(instance);
        registration.Named(DependoHelper.StringifyKey(key));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceKeyed(Type serviceType, object instance, object key)
    {
        var registration = NativeRegistry.For(serviceType).Use(instance.GetType());
        registration.Named(DependoHelper.StringifyKey(key));
        return this;
    }

    private ServiceLifetime GetLifetime(ServiceLifetime lifetime) => lifetime;
}