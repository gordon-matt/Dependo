using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.Autofac;

/// <summary>
/// Autofac implementation of the Dependo engine
/// </summary>
public class AutofacEngine : IEngine, IDisposable
{
    #region Private Members

    private AutofacContainerManager? containerManager;
    private bool disposed;

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
    /// <param name="containerBuilder">Container builder from Autofac</param>
    /// <param name="configuration">Configuration root of the application</param>
    /// <returns>Service provider</returns>
    public virtual IServiceProvider ConfigureServices(ContainerBuilder containerBuilder, IConfigurationRoot configuration)
    {
        // Find startup configurations provided by other assemblies
        var typeFinder = new WebAppTypeFinder();

        // Register dependencies
        RegisterDependencies(containerBuilder, typeFinder);

        // Resolve assemblies here to avoid exceptions when rendering views
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        return ServiceProvider;
    }

    /// <inheritdoc />
    public virtual T Resolve<T>() where T : class =>
        containerManager?.Resolve<T>() ?? throw new InvalidOperationException("Container manager is not initialized");

    /// <inheritdoc />
    public T Resolve<T>(IDictionary<string, object> ctorArgs) where T : class =>
        containerManager?.Resolve<T>(ctorArgs) ?? throw new InvalidOperationException("Container manager is not initialized");

    /// <inheritdoc />
    public virtual object Resolve(Type type) =>
        containerManager?.Resolve(type) ?? throw new InvalidOperationException("Container manager is not initialized");

    /// <inheritdoc />
    public T ResolveNamed<T>(string name) where T : class =>
        containerManager?.ResolveNamed<T>(name) ?? throw new InvalidOperationException("Container manager is not initialized");

    /// <inheritdoc />
    public virtual IEnumerable<T> ResolveAll<T>() =>
        containerManager?.ResolveAll<T>() ?? throw new InvalidOperationException("Container manager is not initialized");

    /// <inheritdoc />
    public IEnumerable<T> ResolveAllNamed<T>(string name) =>
        containerManager?.ResolveAllNamed<T>(name) ?? throw new InvalidOperationException("Container manager is not initialized");

    /// <inheritdoc />
    public virtual object ResolveUnregistered(Type type) =>
        containerManager?.ResolveUnregistered(type) ?? throw new InvalidOperationException("Container manager is not initialized");

    /// <inheritdoc />
    public bool TryResolve<T>(out T? instance) where T : class
    {
        if (containerManager == null)
        {
            instance = default!;
            return false;
        }

        return containerManager.TryResolve(out instance);
    }

    /// <inheritdoc />
    public bool TryResolve(Type serviceType, out object? instance)
    {
        if (containerManager == null)
        {
            instance = default!;
            return false;
        }

        return containerManager.TryResolve(serviceType, out instance);
    }

    #endregion IEngine Members

    #region Non-Public Methods

    /// <summary>
    /// Get IServiceProvider
    /// </summary>
    /// <returns>IServiceProvider</returns>
    protected virtual IServiceProvider GetServiceProvider()
    {
        var httpContextAccessor = ServiceProvider.GetService<IHttpContextAccessor>();
        var context = httpContextAccessor?.HttpContext;
        return context?.RequestServices ?? ServiceProvider;
    }

    /// <summary>
    /// Register dependencies using Autofac
    /// </summary>
    /// <param name="containerBuilder">Container Builder</param>
    /// <param name="typeFinder">Type finder</param>
    protected virtual IServiceProvider RegisterDependencies(ContainerBuilder containerBuilder, ITypeFinder typeFinder)
    {
        // Register engine
        containerBuilder.RegisterInstance(this).As<IEngine>().SingleInstance();

        // Register type finder
        containerBuilder.RegisterInstance(typeFinder).As<ITypeFinder>().SingleInstance();

        // Create an abstraction over the Autofac container builder
        var builder = new AutofacContainerBuilder(containerBuilder);

        // Find dependency registrars provided by other assemblies
        var dependencyRegistrars = typeFinder.FindClassesOfType<IDependencyRegistrar>()
            .Where(x => x.Name != "AutofacDependencyRegistrarAdapter"); // Not working yet.. can't use Activator.CreateInstance here

        // Create and sort instances of dependency registrars
        var instances = dependencyRegistrars
            .Select(x => (IDependencyRegistrar)Activator.CreateInstance(x)!)
            .OrderBy(x => x.Order);

        // Register all provided dependencies
        foreach (var dependencyRegistrar in instances)
        {
            dependencyRegistrar.Register(builder, typeFinder);
        }

        // Create service provider

#pragma warning disable DF0010 // Should not be disposed here.
        var container = containerBuilder.Build();
#pragma warning restore DF0010 //

#pragma warning disable DF0022 // Should not be disposed here.
        ServiceProvider = new AutofacServiceProvider(container);
#pragma warning restore DF0022 //

        containerManager = new AutofacContainerManager(container);
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
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            containerManager?.Dispose();
        }

        disposed = true;
    }

    #endregion IDisposable Members
}