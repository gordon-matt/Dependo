using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.LightInject;

/// <summary>
/// LightInject implementation of the Dependo container
/// </summary>
public class LightInjectDependoContainer : BaseDependoContainer
{
    #region Private Members

    protected IServiceContainer? _container;
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
    /// <param name="container">Service container</param>
    /// <param name="configuration">Configuration root of the application</param>
    /// <returns>Service provider</returns>
    public virtual IServiceProvider ConfigureServices(ServiceContainer container, IConfiguration configuration)
    {
        // Find startup configurations provided by other assemblies
        var typeFinder = new WebAppTypeFinder();

        // Register dependencies
        ServiceProvider = RegisterDependencies(container, typeFinder, configuration);
        _container = container;

        return ServiceProvider;
    }

    #region IDependoContainer Members

    /// <inheritdoc />
    public override bool IsRegistered(Type serviceType)
    {
        if (ServiceProvider == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        return ServiceProvider.GetService(serviceType) != null;
    }

    /// <inheritdoc />
    public override T Resolve<T>() where T : class
    {
        if (ServiceProvider == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        return ServiceProvider.GetService<T>() ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name}");
    }

    /// <inheritdoc />
    public override T Resolve<T>(IDictionary<string, object> ctorArgs) where T : class =>
        throw new NotSupportedException("LightInject does not support passing constructor arguments");

    /// <inheritdoc />
    public override object Resolve(Type type)
    {
        if (ServiceProvider == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        return ServiceProvider.GetService(type) ?? throw new InvalidOperationException($"Could not resolve {type.Name}");
    }

    /// <inheritdoc />
    public override T ResolveKeyed<T>(object key) where T : class => ResolveNamed<T>(DependoHelper.StringifyKey(key));

    /// <inheritdoc />
    public override T ResolveKeyed<T>(object key, IDictionary<string, object> ctorArgs) where T : class =>
        throw new NotSupportedException("LightInject does not support passing constructor arguments");

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAllKeyed<T>(object key) =>
        throw new NotSupportedException(
            "LightInject does not support multiple registrations of the same type and key. When registering, they get overriden. Call ResolveKeyed<T> instead");

    /// <inheritdoc />
    public override T ResolveNamed<T>(string name) where T : class
    {
        if (ServiceProvider == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        return ServiceProvider.GetKeyedService<T>(name);
    }

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAll<T>()
    {
        if (ServiceProvider == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        return ServiceProvider.GetServices<T>();
    }

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAllNamed<T>(string name) =>
        throw new NotSupportedException(
            "LightInject does not support multiple registrations of the same type and name. When registering, they get overriden. Call ResolveNamed<T> instead");

    /// <inheritdoc />
    public override bool TryResolve<T>(out T? instance) where T : class
    {
        if (ServiceProvider == null)
        {
            instance = default;
            return false;
        }

        try
        {
            instance = ServiceProvider.GetService<T>();
            return instance != null;
        }
        catch
        {
            instance = default;
            return false;
        }
    }

    /// <inheritdoc />
    public override bool TryResolve(Type serviceType, out object? instance)
    {
        if (ServiceProvider == null)
        {
            instance = default;
            return false;
        }

        try
        {
            instance = ServiceProvider.GetService(serviceType);
            return instance != null;
        }
        catch
        {
            instance = default;
            return false;
        }
    }

    #endregion IDependoContainer Members

    #region Non-Public Methods

    /// <summary>
    /// Register dependencies
    /// </summary>
    /// <param name="container">Service container</param>
    /// <param name="typeFinder">Type finder</param>
    protected virtual IServiceProvider RegisterDependencies(ServiceContainer container, ITypeFinder typeFinder, IConfiguration configuration)
    {
        // Register dependo container
        container.RegisterInstance(this);

        // Register type finder
        container.RegisterInstance(typeFinder);

        // Create a LightInject container builder abstraction
        var builder = new LightInjectContainerBuilder(container);

        // Find dependency registrars provided by other assemblies
        var dependencyRegistrars = typeFinder.FindClassesOfType<IDependencyRegistrar>()
            .Where(x => !typeof(IDependencyRegistrarAdapter).IsAssignableFrom(x));

        // Find LightInject-specific registrars to create adapters for them
        var lightInjectRegistrars = typeFinder.FindClassesOfType<ILightInjectDependencyRegistrar>();

        // Create and sort instances of dependency registrars
        var instances = dependencyRegistrars
            .Select(x => (IDependencyRegistrar)Activator.CreateInstance(x)!)
            .Concat(lightInjectRegistrars.Select(x => LightInjectDependencyRegistrarAdapter.CreateFromType(x)))
            .OrderBy(x => x.Order);

        // Register all provided dependencies
        foreach (var dependencyRegistrar in instances)
        {
            dependencyRegistrar.Register(builder, typeFinder, configuration);
        }

        return container.CreateServiceProvider(new ServiceCollection());
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
            _container?.Dispose();
        }

        _disposed = true;
    }

    #endregion IDisposable Members
}