using Microsoft.Extensions.DependencyInjection;

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
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped);

    /// <summary>
    /// Registers an open generic type as a service with implementation
    /// </summary>
    /// <param name="serviceType">Service type</param>
    /// <param name="implementationType">Implementation type</param>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterGeneric(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped);

    /// <summary>
    /// Registers a service type with implementation
    /// </summary>
    /// <typeparam name="TService">Service type</typeparam>
    /// <typeparam name="TImplementation">Implementation type</typeparam>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService;

    /// <summary>
    /// Registers a concrete type as itself
    /// </summary>
    /// <typeparam name="TImplementation">Implementation type</typeparam>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterSelf<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class;

    /// <summary>
    /// Registers an instance as a service
    /// </summary>
    /// <typeparam name="TService">Service type</typeparam>
    /// <param name="instance">Service instance</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterInstance<TService>(TService instance)
        where TService : class;

    /// <summary>
    /// Registers an instance as a service
    /// </summary>
    /// <param name="serviceType">Service type</param>
    /// <param name="instance">Service instance</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterInstance(Type serviceType, object instance);

    /// <summary>
    /// Registers a type as a service with implementation and a name
    /// </summary>
    /// <param name="serviceType">Service type</param>
    /// <param name="implementationType">Implementation type</param>
    /// <param name="name">Service name</param>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterNamed(Type serviceType, Type implementationType, string name, ServiceLifetime lifetime = ServiceLifetime.Scoped);

    /// <summary>
    /// Registers a service type with implementation and a name
    /// </summary>
    /// <typeparam name="TService">Service type</typeparam>
    /// <typeparam name="TImplementation">Implementation type</typeparam>
    /// <param name="name">Service name</param>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterNamed<TService, TImplementation>(string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService;

    /// <summary>
    /// Registers a concrete type as itself with a name
    /// </summary>
    /// <typeparam name="TImplementation">Implementation type</typeparam>
    /// <param name="name">Service name</param>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterSelfNamed<TImplementation>(string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class;

    /// <summary>
    /// Registers an instance as a service with a name
    /// </summary>
    /// <typeparam name="TService">Service type</typeparam>
    /// <param name="instance">Service instance</param>
    /// <param name="name">Service name</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterInstanceNamed<TService>(TService instance, string name)
        where TService : class;

    /// <summary>
    /// Registers an instance as a service with a name
    /// </summary>
    /// <param name="serviceType">Service type</param>
    /// <param name="instance">Service instance</param>
    /// <param name="name">Service name</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterInstanceNamed(Type serviceType, object instance, string name);

    /// <summary>
    /// Registers a type as a service with implementation and a key
    /// </summary>
    /// <param name="serviceType">Service type</param>
    /// <param name="implementationType">Implementation type</param>
    /// <param name="key">Service key</param>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterKeyed(Type serviceType, Type implementationType, object key, ServiceLifetime lifetime = ServiceLifetime.Scoped);

    /// <summary>
    /// Registers a service type with implementation and a key
    /// </summary>
    /// <typeparam name="TService">Service type</typeparam>
    /// <typeparam name="TImplementation">Implementation type</typeparam>
    /// <param name="key">Service key</param>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterKeyed<TService, TImplementation>(object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService;

    /// <summary>
    /// Registers a concrete type as itself with a key
    /// </summary>
    /// <typeparam name="TImplementation">Implementation type</typeparam>
    /// <param name="key">Service key</param>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterSelfKeyed<TImplementation>(object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class;

    /// <summary>
    /// Registers an instance as a service with a key
    /// </summary>
    /// <typeparam name="TService">Service type</typeparam>
    /// <param name="instance">Service instance</param>
    /// <param name="key">Service key</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterInstanceKeyed<TService>(TService instance, object key)
        where TService : class;

    /// <summary>
    /// Registers an instance as a service with a key
    /// </summary>
    /// <param name="serviceType">Service type</param>
    /// <param name="instance">Service instance</param>
    /// <param name="key">Service key</param>
    /// <returns>Container builder for chaining</returns>
    IContainerBuilder RegisterInstanceKeyed(Type serviceType, object instance, object key);
}