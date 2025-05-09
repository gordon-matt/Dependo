namespace Dependo;

/// <summary>
/// Interface for the Dependo engine that serves as the main entry point for dependency resolution
/// </summary>
public interface IEngine
{
    /// <summary>
    /// Resolve service of specified type
    /// </summary>
    /// <typeparam name="T">Type of service to resolve</typeparam>
    /// <returns>Resolved service</returns>
    T Resolve<T>() where T : class;

    /// <summary>
    /// Resolve service of specified type with constructor arguments
    /// </summary>
    /// <typeparam name="T">Type of service to resolve</typeparam>
    /// <param name="ctorArgs">Constructor arguments</param>
    /// <returns>Resolved service</returns>
    T Resolve<T>(IDictionary<string, object> ctorArgs) where T : class;

    /// <summary>
    /// Resolve service by type
    /// </summary>
    /// <param name="type">Type of service to resolve</param>
    /// <returns>Resolved service</returns>
    object Resolve(Type type);

    /// <summary>
    /// Resolve named service
    /// </summary>
    /// <typeparam name="T">Type of service to resolve</typeparam>
    /// <param name="name">Service name</param>
    /// <returns>Resolved service</returns>
    T ResolveNamed<T>(string name) where T : class;

    /// <summary>
    /// Resolve all services of the specified type
    /// </summary>
    /// <typeparam name="T">Type of services to resolve</typeparam>
    /// <returns>All resolved services</returns>
    IEnumerable<T> ResolveAll<T>();

    /// <summary>
    /// Resolve all named services of the specified type
    /// </summary>
    /// <typeparam name="T">Type of services to resolve</typeparam>
    /// <param name="name">Service name</param>
    /// <returns>All resolved named services</returns>
    IEnumerable<T> ResolveAllNamed<T>(string name);

    /// <summary>
    /// Resolve service that was not previously registered
    /// </summary>
    /// <param name="type">Type of service to resolve</param>
    /// <returns>Resolved service</returns>
    object ResolveUnregistered(Type type);

    /// <summary>
    /// Try to resolve service of specified type
    /// </summary>
    /// <typeparam name="T">Type of service to resolve</typeparam>
    /// <param name="instance">Resolved service</param>
    /// <returns>True if resolved successfully</returns>
    bool TryResolve<T>(out T instance) where T : class;

    /// <summary>
    /// Try to resolve service by type
    /// </summary>
    /// <param name="serviceType">Type of service to resolve</param>
    /// <param name="instance">Resolved service</param>
    /// <returns>True if resolved successfully</returns>
    bool TryResolve(Type serviceType, out object instance);
}