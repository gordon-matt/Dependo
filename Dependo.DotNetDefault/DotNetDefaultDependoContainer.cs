using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.DotNetDefault;

/// <summary>
/// .NET Default implementation of the Dependo container
/// </summary>
public class DotNetDefaultDependoContainer : BaseDependoContainer
{
    #region Private Members

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
    public virtual IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Find startup configurations provided by other assemblies
        var typeFinder = new WebAppTypeFinder();

        // Register dependencies
        ServiceProvider = RegisterDependencies(services, typeFinder, configuration);
        return ServiceProvider;
    }

    #region IDependoContainer Members

    /// <inheritdoc />
    public override bool IsRegistered(Type serviceType)
    {
        var serviceProviderIsService = ServiceProvider.GetService<IServiceProviderIsService>();
        return serviceProviderIsService is null
            ? throw new InvalidOperationException("IServiceProviderIsService is not available")
            : serviceProviderIsService.IsService(serviceType);
    }

    /// <inheritdoc />
    public override T Resolve<T>() where T : class =>
        ServiceProvider.GetService<T>() ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name}");

    /// <inheritdoc />
    public override T Resolve<T>(IDictionary<string, object> ctorArgs) where T : class =>
        throw new NotSupportedException(".NET Default DI does not support passing constructor arguments");

    /// <inheritdoc />
    public override object Resolve(Type type) =>
        ServiceProvider.GetService(type) ?? throw new InvalidOperationException($"Could not resolve {type.Name}");

    /// <inheritdoc />
    public override T ResolveKeyed<T>(object key) where T : class =>
        ServiceProvider.GetKeyedService<T>(key) ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name}");

    /// <inheritdoc />
    public override T ResolveKeyed<T>(object key, IDictionary<string, object> ctorArgs) where T : class =>
        throw new NotSupportedException(".NET Default DI does not support passing constructor arguments");

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAllKeyed<T>(object key) => ServiceProvider.GetKeyedServices<T>(key);

    /// <inheritdoc />
    public override T ResolveNamed<T>(string name) where T : class =>
        ServiceProvider.GetKeyedService<T>(name) ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name}");

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAll<T>() => ServiceProvider.GetServices<T>();

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAllNamed<T>(string name) => ServiceProvider.GetKeyedServices<T>(name);

    /// <inheritdoc />
    public override bool TryResolve<T>(out T? instance) where T : class
    {
        instance = ServiceProvider.GetService<T>();
        return instance != null;
    }

    /// <inheritdoc />
    public override bool TryResolve(Type serviceType, out object? instance)
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
    protected virtual IServiceProvider RegisterDependencies(IServiceCollection services, ITypeFinder typeFinder, IConfiguration configuration)
    {
        // Register dependo container
        services.AddSingleton<IDependoContainer>(this);

        // Register type finder
        services.AddSingleton(typeFinder);

        // Find dependency registrars provided by other assemblies
        var dependencyRegistrars = typeFinder.FindClassesOfType<IDependencyRegistrar>()
            .Where(x => !typeof(IDependencyRegistrarAdapter).IsAssignableFrom(x));

        // Find .NET Default-specific registrars to create adapters for them
        var dotNetDefaultRegistrars = typeFinder.FindClassesOfType<IDotNetDefaultDependencyRegistrar>();

        // Create and sort instances of dependency registrars
        var instances = dependencyRegistrars
            .Select(x => (IDependencyRegistrar)Activator.CreateInstance(x)!)
            .Concat(dotNetDefaultRegistrars.Select(x => DotNetDefaultDependencyRegistrarAdapter.CreateFromType(x)))
            .OrderBy(x => x.Order);

        // Register all provided dependencies
        foreach (var dependencyRegistrar in instances)
        {
            dependencyRegistrar.Register(new DotNetDefaultContainerBuilder(services), typeFinder, configuration);
        }

        return services.BuildServiceProvider();
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
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _disposed = true;
    }

    #endregion IDisposable Members
}