using System.Xml.Linq;
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

    protected Container? _container;
    private bool _disposed;

    #endregion Private Members

    #region Properties

    /// <summary>
    /// Gets or sets service provider
    /// </summary>
    public virtual IServiceProvider ServiceProvider { get; protected set; } = default!;

    #endregion Properties

    /// <summary>
    /// Configure services for the application
    /// </summary>
    /// <param name="serviceRegistry">Service collection</param>
    /// <param name="configuration">Configuration root of the application</param>
    /// <returns>Service provider</returns>
    public virtual IServiceProvider ConfigureServices(ServiceRegistry serviceRegistry, IConfiguration configuration)
    {
        // Find startup configurations provided by other assemblies
        var typeFinder = new WebAppTypeFinder();

        // Register dependencies
        _container = RegisterDependencies(serviceRegistry, typeFinder, configuration) as Container;
        ServiceProvider = _container!;
        return ServiceProvider;
    }

    #region IDependoContainer Members

    /// <inheritdoc />
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
    public override T ResolveKeyed<T>(object key) where T : class =>
        _container == null
            ? throw new InvalidOperationException("Container is not initialized")
            : ResolveNamed<T>(DependoHelper.StringifyKey(key));

    /// <inheritdoc />
    public override T ResolveKeyed<T>(object key, IDictionary<string, object> ctorArgs) where T : class =>
        throw new NotSupportedException("Lamar does not support passing constructor arguments");

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAllKeyed<T>(object key) =>
        throw new NotSupportedException(
            "Lamar does not support multiple registrations of the same type and key. When registering, they get overriden. Call ResolveKeyed<T> instead");

    /// <inheritdoc />
    public override T ResolveNamed<T>(string name) where T : class =>
        _container == null ? throw new InvalidOperationException("Container is not initialized") : _container.GetInstance<T>(name);

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAll<T>() => ServiceProvider.GetServices<T>();

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAllNamed<T>(string name) =>
        throw new NotSupportedException(
            "Lamar does not support multiple registrations of the same type and name. When registering, they get overriden. Call ResolveNamed<T> instead");

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
    /// <param name="serviceRegistry">Service collection</param>
    /// <param name="typeFinder">Type finder</param>
    protected virtual IServiceProvider RegisterDependencies(ServiceRegistry serviceRegistry, ITypeFinder typeFinder, IConfiguration configuration)
    {
        // Register dependo container
        serviceRegistry.AddSingleton<IDependoContainer>(this);

        // Register type finder
        serviceRegistry.AddSingleton(typeFinder);

        // Create a Lamar container builder abstraction
        var builder = new LamarContainerBuilder(serviceRegistry);

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

#pragma warning disable DF0100 // Should not be disposed here.
        var container = new Container(serviceRegistry);
#pragma warning restore DF0100

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