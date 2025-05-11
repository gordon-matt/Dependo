using LightInject;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.LightInject;

/// <summary>
/// LightInject implementation of the container builder abstraction
/// </summary>
public class LightInjectContainerBuilder : IContainerBuilder
{
    /// <summary>
    /// Initializes a new instance of the LightInjectContainerBuilder class
    /// </summary>
    /// <param name="container">LightInject service container</param>
    public LightInjectContainerBuilder(IServiceContainer container)
    {
        NativeContainer = container;
    }

    /// <summary>
    /// Gets the native LightInject service container
    /// </summary>
    public IServiceContainer NativeContainer { get; }

    /// <inheritdoc/>
    public IContainerBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
    {
        var lifetimeType = GetLifetime(lifetime);
        if (!string.IsNullOrEmpty(name))
        {
            if (lifetimeType != null)
            {
                NativeContainer.Register(serviceType, implementationType, name, lifetimeType);
            }
            else
            {
                NativeContainer.Register(serviceType, implementationType, name);
            }
        }
        else
        {
            if (lifetimeType != null)
            {
                NativeContainer.Register(serviceType, implementationType, lifetimeType);
            }
            else
            {
                NativeContainer.Register(serviceType, implementationType);
            }
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
        where TService : class
        where TImplementation : class, TService
    {
        var lifetimeType = GetLifetime(lifetime);
        if (!string.IsNullOrEmpty(name))
        {
            if (lifetimeType != null)
            {
                NativeContainer.Register<TService, TImplementation>(name, lifetimeType);
            }
            else
            {
                NativeContainer.Register<TService, TImplementation>(name);
            }
        }
        else
        {
            if (lifetimeType != null)
            {
                NativeContainer.Register<TService, TImplementation>(lifetimeType);
            }
            else
            {
                NativeContainer.Register<TService, TImplementation>();
            }
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelf<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
        where TImplementation : class
    {
        var lifetimeType = GetLifetime(lifetime);
        if (!string.IsNullOrEmpty(name))
        {
            if (lifetimeType != null)
            {
                NativeContainer.Register<TImplementation, TImplementation>(name, lifetimeType);
            }
            else
            {
                NativeContainer.Register<TImplementation, TImplementation>(name);
            }
        }
        else
        {
            if (lifetimeType != null)
            {
                NativeContainer.Register<TImplementation>(lifetimeType);
            }
            else
            {
                NativeContainer.Register<TImplementation>();
            }
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance<TService>(TService instance, string? name = null)
        where TService : class
    {
        if (!string.IsNullOrEmpty(name))
        {
            NativeContainer.RegisterInstance(instance, name);
        }
        else
        {
            NativeContainer.RegisterInstance(instance);
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance(Type serviceType, object instance, string? name = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            NativeContainer.RegisterInstance(serviceType, instance, name);
        }
        else
        {
            NativeContainer.RegisterInstance(serviceType, instance);
        }
        return this;
    }

    private ILifetime? GetLifetime(ServiceLifetime lifetime) =>
        lifetime switch
        {
            ServiceLifetime.Singleton => new PerContainerLifetime(),
            ServiceLifetime.Scoped => new PerScopeLifetime(),
            ServiceLifetime.Transient => null,
            _ => null
        };
}