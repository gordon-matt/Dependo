using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.Lamar;

/// <summary>
/// Lamar implementation of the Dependo engine
/// </summary>
/// <remarks>
/// This is a stub class to demonstrate the concept.
/// In a real implementation, you would use actual Lamar types and methods.
/// </remarks>
public class LamarEngine : IEngine, IDisposable
{
    #region Private Members

    private readonly object? _container;
    private bool _disposed;

    #endregion Private Members

    #region Properties

    /// <summary>
    /// Gets or sets service provider
    /// </summary>
    public virtual IServiceProvider ServiceProvider { get; private set; } = default!;

    #endregion Properties

    #region IEngine Members

    /// <summary>
    /// Configure services for the application
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration root of the application</param>
    /// <returns>Service provider</returns>
    public virtual IServiceProvider ConfigureServices(IServiceCollection services, IConfigurationRoot configuration)
    {
        // Find startup configurations provided by other assemblies
        var typeFinder = new WebAppTypeFinder();

        // Register dependencies
        RegisterDependencies(services, typeFinder);

        // Build the container (Lamar Container in a real implementation)
        // _container = new Container(services);
        // ServiceProvider = _container;

        // Using default service provider for this stub

#pragma warning disable DF0023 // Should not be disposed here.
        ServiceProvider = services.BuildServiceProvider();
#pragma warning restore DF0023

        return ServiceProvider;
    }

    /// <inheritdoc />
    public virtual T Resolve<T>() where T : class => ServiceProvider.GetService<T>() ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name}");

    /// <inheritdoc />
    public T Resolve<T>(IDictionary<string, object> ctorArgs) where T : class => throw new NotImplementedException("Resolving with constructor arguments not implemented in stub");

    /// <inheritdoc />
    public virtual object Resolve(Type type) => ServiceProvider.GetService(type) ?? throw new InvalidOperationException($"Could not resolve {type.Name}");

    /// <inheritdoc />
    public T ResolveNamed<T>(string name) where T : class => throw new NotImplementedException("Named resolution not implemented in stub");

    /// <inheritdoc />
    public virtual IEnumerable<T> ResolveAll<T>() => ServiceProvider.GetServices<T>();

    /// <inheritdoc />
    public IEnumerable<T> ResolveAllNamed<T>(string name) => throw new NotImplementedException("Named resolution not implemented in stub");

    /// <inheritdoc />
    public virtual object ResolveUnregistered(Type type) => throw new NotImplementedException("Unregistered resolution not implemented in stub");

    /// <inheritdoc />
    public bool TryResolve<T>(out T instance) where T : class
    {
        instance = ServiceProvider.GetService<T>()!;
        return instance != null;
    }

    /// <inheritdoc />
    public bool TryResolve(Type serviceType, out object instance)
    {
        instance = ServiceProvider.GetService(serviceType)!;
        return instance != null;
    }

    #endregion IEngine Members

    #region Non-Public Methods

    /// <summary>
    /// Register dependencies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="typeFinder">Type finder</param>
    protected virtual IServiceProvider RegisterDependencies(IServiceCollection services, ITypeFinder typeFinder)
    {
        // Register engine
        services.AddSingleton<IEngine>(this);

        // Register type finder
        services.AddSingleton(typeFinder);

        // In a real implementation, a LamarRegistry would be created here
        // var registry = new ServiceRegistry();
        // registry.AddRange(services);

        // Create a Lamar container builder abstraction
        var builder = new LamarContainerBuilder(new object());

        // Find dependency registrars provided by other assemblies
        var dependencyRegistrars = typeFinder.FindClassesOfType<IDependencyRegistrar>();

        // Create and sort instances of dependency registrars
        var instances = dependencyRegistrars
            .Select(x => (IDependencyRegistrar)Activator.CreateInstance(x)!)
            .OrderBy(x => x.Order);

        // Register all provided dependencies
        foreach (var dependencyRegistrar in instances)
        {
            dependencyRegistrar.Register(builder, typeFinder);
        }

        // Create service provider (for this stub we're using the default)
        return services.BuildServiceProvider();
    }

    #endregion Non-Public Methods

    #region IDisposable Members

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose engine resources
    /// </summary>
    /// <param name="disposing">Indicates whether the method was called from Dispose()</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // In a real implementation, dispose the container
            (_container as IDisposable)?.Dispose();
        }

        _disposed = true;
    }

    #endregion IDisposable Members
}