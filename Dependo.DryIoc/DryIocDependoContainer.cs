using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.DryIoc;

/// <summary>
/// DryIoc implementation of the Dependo container
/// </summary>
public class DryIocDependoContainer : BaseDependoContainer
{
    #region Private Members

    protected IContainer? _container;
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
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration root of the application</param>
    /// <returns>Service provider</returns>
    public virtual IServiceProvider ConfigureServices(IContainer container, IConfiguration configuration)
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
    public override bool IsRegistered(Type serviceType) => _container.IsRegistered(serviceType);

    /// <inheritdoc />
    public override T Resolve<T>() where T : class
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        //return ServiceProvider.GetService<T>() ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name}");

        return _container.Resolve<T>() ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name}");
    }

    /// <inheritdoc />
    public override T Resolve<T>(IDictionary<string, object> ctorArgs) where T : class
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        // Convert dictionary to array of parameters
        object[] args = ctorArgs.Values.ToArray();
        return _container.Resolve<T>(args);
    }

    /// <inheritdoc />
    public override object Resolve(Type type)
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        //return ServiceProvider.GetService(type) ?? throw new InvalidOperationException($"Could not resolve {type.Name}");

        return _container.Resolve(type) ?? throw new InvalidOperationException($"Could not resolve {type.Name}");
    }

    /// <inheritdoc />
    public override T ResolveKeyed<T>(object key) where T : class
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        //return ServiceProvider.GetKeyedService<T>(key)
        //    ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name} with key {key}");

        return _container.Resolve<T>(serviceKey: key);
    }

    /// <inheritdoc />
    public override T ResolveKeyed<T>(object key, IDictionary<string, object> ctorArgs) where T : class
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        // Convert dictionary to array of parameters
        object[] args = ctorArgs.Values.ToArray();
        return _container.Resolve<T>(args, serviceKey: key);
    }

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAllKeyed<T>(object key) =>
        throw new NotSupportedException("DryIOC does not support multiple registrations of the same type and key. When registering, an exception is thrown.");

    /// <inheritdoc />
    public override T ResolveNamed<T>(string name) where T : class
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        //return ServiceProvider.GetKeyedService<T>(name)
        //    ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name} with name {name}");

        return _container.Resolve<T>(serviceKey: name);
    }

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAll<T>()
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        //return ServiceProvider.GetServices<T>()
        //    ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name}");

        return _container.ResolveMany<T>().ToList();
    }

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAllNamed<T>(string name) =>
        throw new NotSupportedException("DryIOC does not support multiple registrations of the same type and name. When registering, an exception is thrown.");

    /// <inheritdoc />
    public override bool TryResolve<T>(out T? instance) where T : class
    {
        instance = ServiceProvider.GetService<T>();
        return instance != null;

        //instance = _container!.Resolve<T>(IfUnresolved.ReturnDefault)!;
        //return instance != default;
    }

    /// <inheritdoc />
    public override bool TryResolve(Type serviceType, out object? instance)
    {
        instance = ServiceProvider.GetService(serviceType);
        return instance != null;

        //instance = _container!.Resolve(serviceType, IfUnresolved.ReturnDefault);
        //return instance != default;
    }

    #endregion IDependoContainer Members

    #region Non-Public Methods

    /// <summary>
    /// Register dependencies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="typeFinder">Type finder</param>
    protected virtual IServiceProvider RegisterDependencies(IContainer container, ITypeFinder typeFinder, IConfiguration configuration)
    {
        // Register dependo container
        container.RegisterInstance(this);

        // Register type finder
        container.RegisterInstance(typeFinder);

        // Create a DryIoc container builder abstraction
        var builder = new DryIocContainerBuilder(container);

        // Find dependency registrars provided by other assemblies
        var dependencyRegistrars = typeFinder.FindClassesOfType<IDependencyRegistrar>()
            .Where(x => !typeof(IDependencyRegistrarAdapter).IsAssignableFrom(x));

        // Find DryIoc-specific registrars to create adapters for them
        var dryIocRegistrars = typeFinder.FindClassesOfType<IDryIocDependencyRegistrar>();

        // Create and sort instances of dependency registrars
        var instances = dependencyRegistrars
            .Select(x => (IDependencyRegistrar)Activator.CreateInstance(x)!)
            .Concat(dryIocRegistrars.Select(x => DryIocDependencyRegistrarAdapter.CreateFromType(x)))
            .OrderBy(x => x.Order);

        // Register all provided dependencies
        foreach (var dependencyRegistrar in instances)
        {
            dependencyRegistrar.Register(builder, typeFinder, configuration);
        }

        return container.GetServiceProvider();
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