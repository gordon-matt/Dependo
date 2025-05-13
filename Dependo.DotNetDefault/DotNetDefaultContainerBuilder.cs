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
    public IContainerBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Services.Add(new ServiceDescriptor(
                serviceType: serviceType,
                implementationType: implementationType,
                lifetime: lifetime,
                serviceKey: name));
        }

        Services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
        where TService : class
        where TImplementation : class, TService
    {
        if (!string.IsNullOrEmpty(name))
        {
            Services.Add(new ServiceDescriptor(
                serviceType: typeof(TService),
                implementationType: typeof(TImplementation),
                lifetime: lifetime,
                serviceKey: name));
        }

        Services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelf<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
        where TImplementation : class
    {
        if (!string.IsNullOrEmpty(name))
        {
            Services.Add(new ServiceDescriptor(
                serviceType: typeof(TImplementation),
                implementationType: typeof(TImplementation),
                lifetime: lifetime,
                serviceKey: name));
        }

        Services.Add(new ServiceDescriptor(typeof(TImplementation), typeof(TImplementation), lifetime));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance<TService>(TService instance, string? name = null)
        where TService : class
    {
        if (!string.IsNullOrEmpty(name))
        {
            Services.Add(new ServiceDescriptor(
                serviceType: typeof(TService),
                instance: instance,
                serviceKey: name));
        }

        Services.Add(new ServiceDescriptor(typeof(TService), instance));
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance(Type serviceType, object instance, string? name = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            Services.Add(new ServiceDescriptor(
                serviceType: serviceType,
                instance: instance,
                serviceKey: name));
        }

        Services.Add(new ServiceDescriptor(serviceType, instance));
        return this;
    }
}