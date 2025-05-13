using System.Reflection;
using System.Xml.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Configuration;

namespace Dependo.Autofac;

/// <summary>
/// Autofac implementation of the Dependo container
/// </summary>
public class AutofacDependoContainer : BaseDependoContainer
{
    #region Private Members

    private IContainer _container;
    private bool isDisposed;

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
    /// <param name="containerBuilder">Container builder from Autofac</param>
    /// <param name="configuration">Configuration root of the application</param>
    /// <returns>Service provider</returns>
    public virtual IServiceProvider ConfigureServices(ContainerBuilder containerBuilder, IConfiguration configuration)
    {
        // Find startup configurations provided by other assemblies
        var typeFinder = new WebAppTypeFinder();

        // Register dependencies
        RegisterDependencies(containerBuilder, typeFinder, configuration);

        // Resolve assemblies here to avoid exceptions when rendering views
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        return ServiceProvider;
    }

    #region IDependoContainer Members

    public override bool IsRegistered(Type serviceType) => _container.IsRegistered(serviceType);

    /// <inheritdoc />
    public override T Resolve<T>() where T : class
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        return _container?.Resolve<T>() ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name}");
    }

    /// <inheritdoc />
    public override T Resolve<T>(IDictionary<string, object> ctorArgs) where T : class
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        var ctorParams = ctorArgs.Select(x => new NamedParameter(x.Key, x.Value)).ToArray();
        return _container.Resolve<T>(ctorParams) ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name}");
    }

    /// <inheritdoc />
    public override object Resolve(Type type)
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        return _container?.Resolve(type) ?? throw new InvalidOperationException($"Could not resolve {type.Name}");
    }

    /// <inheritdoc />
    public override T ResolveNamed<T>(string name) where T : class
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        return _container?.ResolveNamed<T>(name)
            ?? throw new InvalidOperationException($"Could not resolve {typeof(T).Name} with name '{name}'");
    }

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAll<T>()
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        return _container?.Resolve<IEnumerable<T>>()
            ?? throw new InvalidOperationException($"Could not resolve any {typeof(T).Name}");
    }

    /// <inheritdoc />
    public override IEnumerable<T> ResolveAllNamed<T>(string name)
    {
        if (_container == null)
        {
            throw new InvalidOperationException("Container is not initialized");
        }

        return _container?.ResolveNamed<IEnumerable<T>>(name)
            ?? throw new InvalidOperationException($"Could not resolve any {typeof(T).Name} with name '{name}'");
    }

    /// <inheritdoc />
    public override bool TryResolve<T>(out T? instance) where T : class
    {
        if (_container == null)
        {
            instance = default!;
            return false;
        }

        return _container.TryResolve(out instance);
    }

    /// <inheritdoc />
    public override bool TryResolve(Type serviceType, out object? instance)
    {
        if (_container == null)
        {
            instance = default!;
            return false;
        }

        return _container.TryResolve(serviceType, out instance);
    }

    #endregion IDependoContainer Members

    #region See if these can be added to all implementations:

    public T ResolveKeyed<T>(object key) where T : class =>  _container.ResolveKeyed<T>(key);

    public T ResolveKeyed<T>(object key, IDictionary<string, object> ctorArgs) where T : class
    {
        var ctorParams = ctorArgs.Select(x => new NamedParameter(x.Key, x.Value)).ToArray();
        return _container.ResolveKeyed<T>(key, ctorParams);
    }

    public IEnumerable<T> ResolveAllKeyed<T>(object key) => _container.ResolveKeyed<IEnumerable<T>>(key);

    #endregion

    #region Non-Public Methods

    /// <summary>
    /// Register dependencies using Autofac
    /// </summary>
    /// <param name="containerBuilder">Container Builder</param>
    /// <param name="typeFinder">Type finder</param>
    protected virtual IServiceProvider RegisterDependencies(ContainerBuilder containerBuilder, ITypeFinder typeFinder, IConfiguration configuration)
    {
        // Register dependo container
        containerBuilder.RegisterInstance(this).As<IDependoContainer>().SingleInstance();

        // Register type finder
        containerBuilder.RegisterInstance(typeFinder).As<ITypeFinder>().SingleInstance();

        // Create an abstraction over the Autofac container builder
        var builder = new AutofacContainerBuilder(containerBuilder);

        // Find dependency registrars provided by other assemblies
        var dependencyRegistrars = typeFinder.FindClassesOfType<IDependencyRegistrar>()
            .Where(x => !typeof(IDependencyRegistrarAdapter).IsAssignableFrom(x));

        // Find Autofac-specific registrars to create adapters for them
        var autofacRegistrars = typeFinder.FindClassesOfType<IAutofacDependencyRegistrar>();

        //var autofacRegistrars = typeFinder.FindClassesOfType<IAutofacDependencyRegistrar>()
        //    .Where(x => !typeof(IDependencyRegistrar).IsAssignableFrom(x) ||
        //        x.Name != "AutofacDependencyRegistrarAdapter");

        // Create and sort instances of dependency registrars
        var instances = new List<IDependencyRegistrar>();

        // Add regular dependency registrars
        instances.AddRange(dependencyRegistrars
            .Select(x => (IDependencyRegistrar)Activator.CreateInstance(x)!));

        // Add Autofac-specific registrars through the adapter
        instances.AddRange(autofacRegistrars
            .Select(x => (IDependencyRegistrar)AutofacDependencyRegistrarAdapter.CreateFromType(x)));

        // Sort by order
        var orderedInstances = instances.OrderBy(x => x.Order);

        // Register all provided dependencies
        foreach (var dependencyRegistrar in orderedInstances)
        {
            dependencyRegistrar.Register(builder, typeFinder, configuration);
        }

        // Create service provider

#pragma warning disable DF0010 // Should not be disposed here.
        var container = containerBuilder.Build();
#pragma warning restore DF0010 //

        _container = container;

#pragma warning disable DF0022 // Should not be disposed here.
        ServiceProvider = new AutofacServiceProvider(container);
#pragma warning restore DF0022 //

        return ServiceProvider;
    }

    private Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        // Check for assembly already loaded
        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
        if (assembly != null)
        {
            return assembly;
        }

        // Get assembly from TypeFinder
        var typeFinder = Resolve<ITypeFinder>();
        return typeFinder.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
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
        if (isDisposed)
        {
            return;
        }

        if (disposing)
        {
            _container?.Dispose();
        }

        isDisposed = true;
    }

    #endregion IDisposable Members
}