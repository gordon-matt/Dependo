using Autofac;

namespace Dependo.Autofac;

/// <summary>
/// Autofac container manager implementation
/// </summary>
internal class AutofacContainerManager : IDisposable
{
    private bool isDisposed;

    /// <summary>
    /// Creates a new instance of the AutofacContainerManager
    /// </summary>
    /// <param name="container">The Autofac container</param>
    public AutofacContainerManager(IContainer container)
    {
        Container = container;
    }

    /// <summary>
    /// Gets the Autofac container
    /// </summary>
    public IContainer Container { get; }

    /// <summary>
    /// Determines whether a type is registered in the container
    /// </summary>
    /// <param name="serviceType">Type to check for registration</param>
    /// <returns>True if the type is registered</returns>
    public bool IsRegistered(Type serviceType) => Container.IsRegistered(serviceType);

    /// <summary>
    /// Resolves a service from the container
    /// </summary>
    /// <typeparam name="T">The type to resolve</typeparam>
    /// <param name="key">Optional key for keyed service</param>
    /// <returns>The resolved instance</returns>
    public T Resolve<T>(string key = "") where T : class =>
        string.IsNullOrEmpty(key) ? Container.Resolve<T>() : Container.ResolveKeyed<T>(key);

    /// <summary>
    /// Resolves a service with constructor parameters
    /// </summary>
    /// <typeparam name="T">The type to resolve</typeparam>
    /// <param name="ctorArgs">Constructor parameters</param>
    /// <param name="key">Optional key for keyed service</param>
    /// <returns>The resolved instance</returns>
    public T Resolve<T>(IDictionary<string, object> ctorArgs, string key = "") where T : class
    {
        var ctorParams = ctorArgs.Select(x => new NamedParameter(x.Key, x.Value)).ToArray();
        return string.IsNullOrEmpty(key)
            ? Container.Resolve<T>(ctorParams)
            : Container.ResolveKeyed<T>(key, ctorParams);
    }

    /// <summary>
    /// Resolves a service from the container by type
    /// </summary>
    /// <param name="type">The type to resolve</param>
    /// <returns>The resolved instance</returns>
    public object Resolve(Type type) => Container.Resolve(type);

    /// <summary>
    /// Resolves all instances of a service
    /// </summary>
    /// <typeparam name="T">The type to resolve</typeparam>
    /// <param name="key">Optional key for keyed service</param>
    /// <returns>All resolved instances</returns>
    public IEnumerable<T> ResolveAll<T>(string key = "") =>
        string.IsNullOrEmpty(key)
            ? Container.Resolve<IEnumerable<T>>().ToArray()
            : Container.ResolveKeyed<IEnumerable<T>>(key).ToArray();

    /// <summary>
    /// Resolves all named instances of a service
    /// </summary>
    /// <typeparam name="T">The type to resolve</typeparam>
    /// <param name="name">The service name</param>
    /// <returns>All resolved named instances</returns>
    public IEnumerable<T> ResolveAllNamed<T>(string name) =>
        Container.ResolveKeyed<IEnumerable<T>>(name).ToArray();

    /// <summary>
    /// Resolves a named service
    /// </summary>
    /// <typeparam name="T">The type to resolve</typeparam>
    /// <param name="name">Service name</param>
    /// <returns>The resolved instance</returns>
    public T ResolveNamed<T>(string name) where T : class =>
        Container.ResolveNamed<T>(name);

    /// <summary>
    /// Resolves an optional service (returns null if not registered)
    /// </summary>
    /// <param name="serviceType">Type to resolve</param>
    /// <returns>The resolved instance or null</returns>
    public object? ResolveOptional(Type serviceType) =>
        Container.ResolveOptional(serviceType);

    /// <summary>
    /// Resolves a service that isn't registered in the container
    /// </summary>
    /// <typeparam name="T">The type to resolve</typeparam>
    /// <returns>The resolved instance</returns>
    public T? ResolveUnregistered<T>() where T : class =>
        ResolveUnregistered(typeof(T)) as T;

    /// <summary>
    /// Resolves a service that isn't registered in the container by trying to create an instance
    /// with dependencies resolved from the container
    /// </summary>
    /// <param name="type">The type to resolve</param>
    /// <returns>The resolved instance</returns>
    /// <exception cref="ApplicationException">Thrown when no suitable constructor is found</exception>
    public object ResolveUnregistered(Type type)
    {
        Exception? innerException = null;
        foreach (var constructor in type.GetConstructors())
        {
            try
            {
                // Try to resolve constructor parameters
                var parameters = constructor.GetParameters().Select(parameter =>
                {
                    var service = Resolve(parameter.ParameterType);
                    return service ?? throw new ApplicationException("Unable to resolve dependency");
                });

                // Create instance with resolved parameters
                return Activator.CreateInstance(type, parameters.ToArray())!;
            }
            catch (Exception ex)
            {
                innerException = ex;
            }
        }
        throw new ApplicationException("No constructor was found that had all the dependencies satisfied.", innerException);
    }

    /// <summary>
    /// Attempts to resolve a service
    /// </summary>
    /// <typeparam name="T">The type to resolve</typeparam>
    /// <param name="instance">The resolved instance</param>
    /// <returns>True if resolved successfully</returns>
    public bool TryResolve<T>(out T? instance) where T : class =>
        Container.TryResolve(out instance);

    /// <summary>
    /// Attempts to resolve a service by type
    /// </summary>
    /// <param name="serviceType">Type to resolve</param>
    /// <param name="instance">The resolved instance</param>
    /// <returns>True if resolved successfully</returns>
    public bool TryResolve(Type serviceType, out object? instance) =>
        Container.TryResolve(serviceType, out instance);

    #region IDisposable Members

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes container resources
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
            Container.Dispose();
        }

        isDisposed = true;
    }

    #endregion IDisposable Members
}