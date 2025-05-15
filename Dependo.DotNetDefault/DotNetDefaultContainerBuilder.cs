using Microsoft.Extensions.DependencyInjection;

namespace Dependo.DotNetDefault;

/// <summary>
/// .NET Default implementation of the container builder abstraction
/// </summary>
public class DotNetDefaultContainerBuilder : IContainerBuilder
{
    /// <summary>
    /// Initializes a new instance of the DotNetDefaultContainerBuilder class
    /// </summary>
    /// <param name="services">Service collection</param>
    public DotNetDefaultContainerBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Gets the service collection
    /// </summary>
    public IServiceCollection Services { get; }

    /// <inheritdoc/>
    public IContainerBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        Services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterGeneric(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        Services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        Services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelf<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        Services.Add(new ServiceDescriptor(typeof(TImplementation), typeof(TImplementation), lifetime));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance<TService>(TService instance)
        where TService : class
    {
        Services.Add(new ServiceDescriptor(typeof(TService), instance));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance(Type serviceType, object instance)
    {
        Services.Add(new ServiceDescriptor(serviceType, instance));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterNamed(Type serviceType, Type implementationType, string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        Services.Add(new ServiceDescriptor(
            serviceType: serviceType,
            implementationType: implementationType,
            lifetime: lifetime,
            serviceKey: name));

        // Additionally, register the service without a name for compatibility when switching between service providers
        Register(serviceType, implementationType, lifetime);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterNamed<TService, TImplementation>(string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        Services.Add(new ServiceDescriptor(
            serviceType: typeof(TService),
            implementationType: typeof(TImplementation),
            lifetime: lifetime,
            serviceKey: name));

        // Additionally, register the service without a name for compatibility when switching between service providers
        Register<TService, TImplementation>(lifetime);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelfNamed<TImplementation>(string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        Services.Add(new ServiceDescriptor(
            serviceType: typeof(TImplementation),
            implementationType: typeof(TImplementation),
            lifetime: lifetime,
            serviceKey: name));

        // Additionally, register the service without a name for compatibility when switching between service providers
        RegisterSelf<TImplementation>(lifetime);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceNamed<TService>(TService instance, string name)
        where TService : class
    {
        Services.Add(new ServiceDescriptor(
            serviceType: typeof(TService),
            instance: instance,
            serviceKey: name));

        // Additionally, register the service without a name for compatibility when switching between service providers
        RegisterInstance(instance);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceNamed(Type serviceType, object instance, string name)
    {
        Services.Add(new ServiceDescriptor(
            serviceType: serviceType,
            instance: instance,
            serviceKey: name));

        // Additionally, register the service without a name for compatibility when switching between service providers
        RegisterInstance(serviceType, instance);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterKeyed(Type serviceType, Type implementationType, object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        Services.Add(new ServiceDescriptor(
            serviceType: serviceType,
            implementationType: implementationType,
            lifetime: lifetime,
            serviceKey: key));

        // Additionally, register the service without a name for compatibility when switching between service providers
        Register(serviceType, implementationType, lifetime);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterKeyed<TService, TImplementation>(object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        Services.Add(new ServiceDescriptor(
            serviceType: typeof(TService),
            implementationType: typeof(TImplementation),
            lifetime: lifetime,
            serviceKey: key));

        // Additionally, register the service without a name for compatibility when switching between service providers
        Register<TService, TImplementation>(lifetime);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelfKeyed<TImplementation>(object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        Services.Add(new ServiceDescriptor(
            serviceType: typeof(TImplementation),
            implementationType: typeof(TImplementation),
            lifetime: lifetime,
            serviceKey: key));

        // Additionally, register the service without a name for compatibility when switching between service providers
        RegisterSelf<TImplementation>(lifetime);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceKeyed<TService>(TService instance, object key)
        where TService : class
    {
        Services.Add(new ServiceDescriptor(
            serviceType: typeof(TService),
            instance: instance,
            serviceKey: key));

        // Additionally, register the service without a name for compatibility when switching between service providers
        RegisterInstance(instance);

        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceKeyed(Type serviceType, object instance, object key)
    {
        Services.Add(new ServiceDescriptor(
            serviceType: serviceType,
            instance: instance,
            serviceKey: key));

        // Additionally, register the service without a name for compatibility when switching between service providers
        RegisterInstance(serviceType, instance);

        return this;
    }
}