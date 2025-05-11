using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.DotNetDefault;

/// <summary>
/// .NET Default implementation of the Dependo container
/// </summary>
public class DotNetDefaultDependoContainer : IDependoContainer, IDisposable
{
    #region Private Members

    private IServiceProvider? _serviceProvider;
    private bool _disposed;

    #endregion Private Members

    #region Properties

    /// <summary>
    /// Gets or sets service provider
    /// </summary>
    public virtual IServiceProvider ServiceProvider { get; private set; } = default!;

    #endregion Properties

    #region IDependoContainer Members

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
        ServiceProvider = RegisterDependencies(services, typeFinder);
        _serviceProvider = ServiceProvider;

        return ServiceProvider;
    }

    /// <inheritdoc />
    public virtual T Resolve<T>() where T : class =>
        ServiceProvider.GetService<T>() ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name}");

    /// <inheritdoc />
    public T Resolve<T>(IDictionary<string, object> ctorArgs) where T : class =>
        throw new NotSupportedException(".NET Default DI does not support passing constructor arguments");

    /// <inheritdoc />
    public virtual object Resolve(Type type) =>
        ServiceProvider.GetService(type) ?? throw new InvalidOperationException($"Could not resolve {type.Name}");

    /// <inheritdoc />
    public T ResolveNamed<T>(string name) where T : class =>
        throw new NotSupportedException(".NET Default DI does not support named services");

    /// <inheritdoc />
    public virtual IEnumerable<T> ResolveAll<T>() => ServiceProvider.GetServices<T>();

    /// <inheritdoc />
    public IEnumerable<T> ResolveAllNamed<T>(string name) =>
        throw new NotSupportedException(".NET Default DI does not support named services");

    /// <inheritdoc />
    public virtual object ResolveUnregistered(Type type) =>
        throw new NotSupportedException(".NET Default DI does not support resolving unregistered services");

    /// <inheritdoc />
    public bool TryResolve<T>(out T? instance) where T : class
    {
        instance = ServiceProvider.GetService<T>();
        return instance != null;
    }

    /// <inheritdoc />
    public bool TryResolve(Type serviceType, out object? instance)
    {
        instance = ServiceProvider.GetService(serviceType);
        return instance != null;
    }

    #endregion IDependoContainer Members

    #region Non-Public Methods

    /// <summary>
    /// Register dependencies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="typeFinder">Type finder</param>
    protected virtual IServiceProvider RegisterDependencies(IServiceCollection services, ITypeFinder typeFinder)
    {
        // Register dependo container
        services.AddSingleton<IDependoContainer>(this);

        // Register type finder
        services.AddSingleton(typeFinder);

        // Find dependency registrars provided by other assemblies
        var dependencyRegistrars = typeFinder.FindClassesOfType<IDependencyRegistrar>()
            .Where(x => !typeof(IDependencyRegistrarAdapter).IsAssignableFrom(x));

        // Create and sort instances of dependency registrars
        var instances = dependencyRegistrars
            .Select(x => (IDependencyRegistrar)Activator.CreateInstance(x)!)
            .OrderBy(x => x.Order);

        // Register all provided dependencies
        foreach (var dependencyRegistrar in instances)
        {
            dependencyRegistrar.Register(new DotNetDefaultContainerBuilder(services), typeFinder);
        }

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
    /// Dispose dependo container resources
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
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _disposed = true;
    }

    #endregion IDisposable Members
} 