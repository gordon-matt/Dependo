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
    public IContainerBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
    {
        var registration = NativeRegistry.For(serviceType).Use(implementationType);
        registration.Lifetime = GetLifetime(lifetime);
        if (!string.IsNullOrEmpty(name))
        {
            registration.Named(name);
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
        where TService : class
        where TImplementation : class, TService
    {
        var registration = NativeRegistry.For<TService>().Use<TImplementation>();
        registration.Lifetime = GetLifetime(lifetime);
        if (!string.IsNullOrEmpty(name))
        {
            registration.Named(name);
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelf<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
        where TImplementation : class
    {
        var registration = NativeRegistry.For<TImplementation>().Use<TImplementation>();
        registration.Lifetime = GetLifetime(lifetime);
        if (!string.IsNullOrEmpty(name))
        {
            registration.Named(name);
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance<TService>(TService instance, string? name = null)
        where TService : class
    {
        var registration = NativeRegistry.For<TService>().Use(instance);
        if (!string.IsNullOrEmpty(name))
        {
            registration.Named(name);
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance(Type serviceType, object instance, string? name = null)
    {
        var registration = NativeRegistry.For(serviceType).Use(instance.GetType());
        if (!string.IsNullOrEmpty(name))
        {
            registration.Named(name);
        }
        return this;
    }

    private ServiceLifetime GetLifetime(ServiceLifetime lifetime) => lifetime;
}