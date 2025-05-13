using Autofac;
using Autofac.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.Autofac;

/// <summary>
/// Autofac implementation of the container builder abstraction
/// </summary>
public class AutofacContainerBuilder : IContainerBuilder
{
    /// <summary>
    /// Initializes a new instance of the AutofacContainerBuilder class
    /// </summary>
    /// <param name="containerBuilder">Autofac container builder</param>
    public AutofacContainerBuilder(ContainerBuilder containerBuilder)
    {
        NativeBuilder = containerBuilder;
    }

    /// <summary>
    /// Gets the native Autofac container builder
    /// </summary>
    public ContainerBuilder NativeBuilder { get; }

    /// <inheritdoc/>
    public IContainerBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var registration = NativeBuilder.RegisterType(implementationType).As(serviceType);
        ApplyLifetime(registration, lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        var registration = NativeBuilder.RegisterType<TImplementation>().As<TService>();
        ApplyLifetime(registration, lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelf<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        var registration = NativeBuilder.RegisterType<TImplementation>().AsSelf();
        ApplyLifetime(registration, lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance<TService>(TService instance)
        where TService : class
    {
        NativeBuilder.RegisterInstance(instance).As<TService>().SingleInstance();
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance(Type serviceType, object instance)
    {
        NativeBuilder.RegisterInstance(instance).As(serviceType).SingleInstance();
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterNamed(Type serviceType, Type implementationType, string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var registration = NativeBuilder.RegisterType(implementationType).Named(name, serviceType);
        ApplyLifetime(registration, lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterNamed<TService, TImplementation>(string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        var registration = NativeBuilder.RegisterType<TImplementation>().Named<TService>(name);
        ApplyLifetime(registration, lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelfNamed<TImplementation>(string name, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        var registration = NativeBuilder.RegisterType<TImplementation>().Named<TImplementation>(name);
        ApplyLifetime(registration, lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceNamed<TService>(TService instance, string name)
        where TService : class
    {
        NativeBuilder.RegisterInstance(instance).Named<TService>(name).SingleInstance();
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceNamed(Type serviceType, object instance, string name)
    {
        NativeBuilder.RegisterInstance(instance).Named(name, serviceType).SingleInstance();
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterKeyed(Type serviceType, Type implementationType, object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var registration = NativeBuilder.RegisterType(implementationType).Keyed(key, serviceType);
        ApplyLifetime(registration, lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterKeyed<TService, TImplementation>(object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TService : class
        where TImplementation : class, TService
    {
        var registration = NativeBuilder.RegisterType<TImplementation>().Keyed<TService>(key);
        ApplyLifetime(registration, lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelfKeyed<TImplementation>(object key, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImplementation : class
    {
        var registration = NativeBuilder.RegisterType<TImplementation>().Keyed<TImplementation>(key);
        ApplyLifetime(registration, lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceKeyed<TService>(TService instance, object key)
        where TService : class
    {
        NativeBuilder.RegisterInstance(instance).Keyed<TService>(key).SingleInstance();
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstanceKeyed(Type serviceType, object instance, object key)
    {
        NativeBuilder.RegisterInstance(instance).Keyed(key, serviceType).SingleInstance();
        return this;
    }

    private void ApplyLifetime(IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration, ServiceLifetime lifetime)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                registration.SingleInstance();
                break;
            case ServiceLifetime.Scoped:
                registration.InstancePerLifetimeScope();
                break;
            case ServiceLifetime.Transient:
                registration.InstancePerDependency();
                break;
        }
    }
}