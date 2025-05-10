using Microsoft.Extensions.DependencyInjection;

// TODO: Add RegisterNamed

namespace Dependo;

/// <summary>
/// Abstraction for dependency injection container builders
/// </summary>
public interface IContainerBuilder
{
    /// <summary>
    /// Registers a type as a service with implementation
    /// </summary>
    /// <param name="serviceType">Service type</param>
    /// <param name="implementationType">Implementation type</param>
    /// <param name="lifetime">Service lifetime</param>
    /// <param name="name">Optional service name</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null);

    /// <summary>
    /// Registers a service type with implementation
    /// </summary>
    /// <typeparam name="TService">Service type</typeparam>
    /// <typeparam name="TImplementation">Implementation type</typeparam>
    /// <param name="lifetime">Service lifetime</param>
    /// <param name="name">Optional service name</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
        where TService : class
        where TImplementation : class, TService;

    /// <summary>
    /// Registers a concrete type as itself
    /// </summary>
    /// <typeparam name="TImplementation">Implementation type</typeparam>
    /// <param name="lifetime">Service lifetime</param>
    /// <param name="name">Optional service name</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterSelf<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
        where TImplementation : class;

    /// <summary>
    /// Registers an instance as a service
    /// </summary>
    /// <typeparam name="TService">Service type</typeparam>
    /// <param name="instance">Service instance</param>
    /// <param name="name">Optional service name</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterInstance<TService>(TService instance, string? name = null)
        where TService : class;

    /// <summary>
    /// Registers an instance as a service
    /// </summary>
    /// <param name="serviceType">Service type</param>
    /// <param name="instance">Service instance</param>
    /// <param name="name">Optional service name</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterInstance(Type serviceType, object instance, string? name = null);
}