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
    public IContainerBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var lifetimeType = GetLifetime(lifetime);
        if (lifetimeType != null)
        {
            NativeContainer.Register(serviceType, implementationType, lifetimeType);
        }
        else
        {
            NativeContainer.Register(serviceType, implementationType);
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterGeneric(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var lifetimeType = GetLifetime(lifetime);
        if (lifetimeType != null)
        {
            NativeContainer.Register(serviceType, implementationType, lifetimeType);
        }
        else
        {
            NativeContainer.Register(serviceType, implementationType);
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        var lifetimeType = GetLifetime(lifetime);
        if (lifetimeType != null)
        {
            NativeContainer.Register<TService, TImplementation>(lifetimeType);
        }
        else
        {
            NativeContainer.Register<TService, TImplementation>();
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelf<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        var lifetimeType = GetLifetime(lifetime);
        if (lifetimeType != null)
        {
            NativeContainer.Register<TImplementation>(lifetimeType);
        }
        else
        {
            NativeContainer.Register<TImplementation>();
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance<TService>(TService instance)
        where TService : class
    {
        NativeContainer.RegisterInstance(instance);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance(Type serviceType, object instance)
    {
        NativeContainer.RegisterInstance(serviceType, instance);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterNamed(Type serviceType, Type implementationType, string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var lifetimeType = GetLifetime(lifetime);
        if (lifetimeType != null)
        {
            NativeContainer.Register(serviceType, implementationType, name, lifetimeType);
        }
        else
        {
            NativeContainer.Register(serviceType, implementationType, name);
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterNamed<TService, TImplementation>(string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        var lifetimeType = GetLifetime(lifetime);
        if (lifetimeType != null)
        {
            NativeContainer.Register<TService, TImplementation>(name, lifetimeType);
        }
        else
        {
            NativeContainer.Register<TService, TImplementation>(name);
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelfNamed<TImplementation>(string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        var lifetimeType = GetLifetime(lifetime);
        if (lifetimeType != null)
        {
            NativeContainer.Register<TImplementation, TImplementation>(name, lifetimeType);
        }
        else
        {
            NativeContainer.Register<TImplementation, TImplementation>(name);
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceNamed<TService>(TService instance, string name)
        where TService : class
    {
        NativeContainer.RegisterInstance(instance, name);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceNamed(Type serviceType, object instance, string name)
    {
        NativeContainer.RegisterInstance(serviceType, instance, name);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterKeyed(Type serviceType, Type implementationType, object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var lifetimeType = GetLifetime(lifetime);
        if (lifetimeType != null)
        {
            NativeContainer.Register(serviceType, implementationType, DependoHelper.StringifyKey(key)!, lifetimeType);
        }
        else
        {
            NativeContainer.Register(serviceType, implementationType, DependoHelper.StringifyKey(key)!);
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterKeyed<TService, TImplementation>(object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        var lifetimeType = GetLifetime(lifetime);
        if (lifetimeType != null)
        {
            NativeContainer.Register<TService, TImplementation>(DependoHelper.StringifyKey(key)!, lifetimeType);
        }
        else
        {
            NativeContainer.Register<TService, TImplementation>(DependoHelper.StringifyKey(key)!);
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelfKeyed<TImplementation>(object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        var lifetimeType = GetLifetime(lifetime);
        if (lifetimeType != null)
        {
            NativeContainer.Register<TImplementation, TImplementation>(DependoHelper.StringifyKey(key), lifetimeType);
        }
        else
        {
            NativeContainer.Register<TImplementation, TImplementation>(DependoHelper.StringifyKey(key));
        }
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceKeyed<TService>(TService instance, object key)
        where TService : class
    {
        NativeContainer.RegisterInstance(instance, DependoHelper.StringifyKey(key)!);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceKeyed(Type serviceType, object instance, object key)
    {
        NativeContainer.RegisterInstance(serviceType, instance, DependoHelper.StringifyKey(key)!);
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