using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.Lamar;

/// <summary>
/// Lamar implementation of the Dependo container
/// </summary>
public class LamarDependoContainer : BaseDependoContainer
{
    #region Private Members

    private Container? _container;
    private bool _disposed;

    #endregion Private Members

    #region Properties

    /// <summary>
    /// Gets or sets service provider
    /// </summary>
    public virtual IServiceProvider ServiceProvider { get; private set; } = default!;

    #endregion Properties

    /// <summary>
    /// Configure services for the application
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration root of the application</param>
    /// <returns>Service provider</returns>
    public virtual IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Find startup configurations provided by other assemblies
        var typeFinder = new WebAppTypeFinder();

        // Register dependencies
        _container = RegisterDependencies(services, typeFinder, configuration) as Container;
        ServiceProvider = _container!;

        return ServiceProvider;
    }

    #region IDependoContainer Members

    public override bool IsRegistered(Type serviceType) =>
        _container == null
            ? throw new InvalidOperationException("Container is not initialized")
            : _container.IsService(serviceType);

    /// <inheritdoc />
    public override T Resolve<T>() where T : class =>
        ServiceProvider.GetService<T>() ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name}");

    /// <inheritdoc />
    public override T Resolve<T>(IDictionary<string, object> ctorArgs) where T : class =>
        throw new NotSupportedException("Lamar does not support passing constructor arguments");

    /// <inheritdoc />
    public override object Resolve(Type type) =>
        ServiceProvider.GetService(type) ?? throw new InvalidOperationException($"Could not resolve {type.Name}");

    /// <inheritdoc />
    public override T ResolveNamed<T>(string name) where T : class =>
        _container == null ? throw new InvalidOperationException("Container is not initialized") : _container.GetInstance<T>(name);

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAll<T>() => ServiceProvider.GetServices<T>();

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAllNamed<T>(string name) =>
        throw new NotSupportedException(
            "Lamar does not support multiple named registrations of the same type. When registering, they get overriden. Call ResolveNamed<T> instead");

    /// <inheritdoc />
    public override object ResolveUnregistered(Type type) =>
        _container == null
            ? throw new InvalidOperationException("Container is not initialized")
            : _container.GetInstance(type)
                ?? base.ResolveUnregistered(type);

    /// <inheritdoc />
    public override bool TryResolve<T>(out T instance) where T : class
    {
        instance = ServiceProvider.GetService<T>()!;
        return instance != null;
    }

    /// <inheritdoc />
    public override bool TryResolve(Type serviceType, out object instance)
    {
        instance = ServiceProvider.GetService(serviceType)!;
        return instance != null;
    }

    #endregion IDependoContainer Members

    #region Non-Public Methods

    /// <summary>
    /// Register dependencies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="typeFinder">Type finder</param>
    protected virtual IServiceProvider RegisterDependencies(IServiceCollection services, ITypeFinder typeFinder, IConfiguration configuration)
    {
        // Register dependo container
        services.AddSingleton<IDependoContainer>(this);

        // Register type finder
        services.AddSingleton(typeFinder);

        // Create a Lamar registry
        var registry = new ServiceRegistry();
        registry.AddRange(services);

        // Create a Lamar container builder abstraction
        var builder = new LamarContainerBuilder(registry);

        // Find dependency registrars provided by other assemblies
        var dependencyRegistrars = typeFinder.FindClassesOfType<IDependencyRegistrar>()
            .Where(x => !typeof(IDependencyRegistrarAdapter).IsAssignableFrom(x));

        // Find Lamar-specific registrars to create adapters for them
        var lamarRegistrars = typeFinder.FindClassesOfType<ILamarDependencyRegistrar>();

        // Create and sort instances of dependency registrars
        var instances = dependencyRegistrars
            .Select(x => (IDependencyRegistrar)Activator.CreateInstance(x)!)
            .Concat(lamarRegistrars.Select(x => LamarDependencyRegistrarAdapter.CreateFromType(x)))
            .OrderBy(x => x.Order);

        // Register all provided dependencies
        foreach (var dependencyRegistrar in instances)
        {
            dependencyRegistrar.Register(builder, typeFinder, configuration);
        }

        // Create container
        var container = new Container(registry);
        return container;
    }

    #endregion Non-Public Methods

    #region IDisposable Members

    /// <inheritdoc />
    public override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose container resources
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
            _container?.Dispose();
        }

        _disposed = true;
    }

    #endregion IDisposable Members
}