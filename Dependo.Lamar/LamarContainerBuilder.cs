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
        NativeRegistry.For(serviceType).Use(implementationType).Lifetime = GetLifetime(lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        NativeRegistry.For<TService>().Use<TImplementation>().Lifetime = GetLifetime(lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelf<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        NativeRegistry.For<TImplementation>().Use<TImplementation>().Lifetime = GetLifetime(lifetime);
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
        // In a real implementation, this would use Lamar's registration API
        // Example: registry.For(serviceType).Use(instance).Singleton();
        Console.WriteLine($"Registering instance of {serviceType.Name}");
        return this;
    }

    private static ServiceLifetime GetLifetime(ServiceLifetime lifetime) => lifetime;
}