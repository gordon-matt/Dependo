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
    /// <param name="registry">Lamar registry</param>
    public LamarContainerBuilder(object registry)
    {
        NativeRegistry = registry;
    }

    /// <summary>
    /// Gets the native Lamar registry
    /// </summary>
    public object NativeRegistry { get; }

    /// <inheritdoc/>
    public IContainerBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        // In a real implementation, this would use Lamar's registration API
        // Example: registry.For(serviceType).Use(implementationType).Lifetime(MapLifetime(lifetime));
        Console.WriteLine($"Registering {implementationType.Name} as {serviceType.Name} with lifetime {lifetime}");
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        // In a real implementation, this would use Lamar's registration API
        // Example: registry.For<TService>().Use<TImplementation>().Lifetime(MapLifetime(lifetime));
        Console.WriteLine($"Registering {typeof(TImplementation).Name} as {typeof(TService).Name} with lifetime {lifetime}");
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelf<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        // In a real implementation, this would use Lamar's registration API
        // Example: registry.ForSelf<TImplementation>().Use<TImplementation>().Lifetime(MapLifetime(lifetime));
        Console.WriteLine($"Registering {typeof(TImplementation).Name} as self with lifetime {lifetime}");
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance<TService>(TService instance)
        where TService : class
    {
        // In a real implementation, this would use Lamar's registration API
        // Example: registry.For<TService>().Use(instance).Singleton();
        Console.WriteLine($"Registering instance of {typeof(TService).Name}");
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

    // In a real implementation, this would map Dependo.Dependency.ServiceLifetime to Lamar's lifetime types
    private string MapLifetime(ServiceLifetime lifetime) => lifetime switch
    {
        ServiceLifetime.Singleton => "Singleton",
        ServiceLifetime.Scoped => "Scoped",
        ServiceLifetime.Transient => "Transient",
        _ => throw new ArgumentOutOfRangeException(nameof(lifetime))
    };
}