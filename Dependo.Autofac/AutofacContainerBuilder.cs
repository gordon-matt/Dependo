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
    public IContainerBuilder Register(Type serviceType, Type implementationType, ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
    {
        var registration = NativeBuilder.RegisterType(implementationType).As(serviceType);
        if (!string.IsNullOrEmpty(name))
        {
            registration.Named(name, serviceType);
        }
        ApplyLifetime(registration, lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder Register<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
        where TService : class
        where TImplementation : class, TService
    {
        var registration = NativeBuilder.RegisterType<TImplementation>().As<TService>();
        if (!string.IsNullOrEmpty(name))
        {
            registration.Named<TService>(name);
        }
        ApplyLifetime(registration, lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterSelf<TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Scoped, string? name = null)
        where TImplementation : class
    {
        var registration = NativeBuilder.RegisterType<TImplementation>().AsSelf();
        if (!string.IsNullOrEmpty(name))
        {
            registration.Named<TImplementation>(name);
        }
        ApplyLifetime(registration, lifetime);
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance<TService>(TService instance, string? name = null)
        where TService : class
    {
        var registration = NativeBuilder.RegisterInstance(instance).As<TService>();
        if (!string.IsNullOrEmpty(name))
        {
            registration.Named<TService>(name);
        }
        registration.SingleInstance();
        return this;
    }

    /// <inheritdoc/>
    public IContainerBuilder RegisterInstance(Type serviceType, object instance, string? name = null)
    {
        var registration = NativeBuilder.RegisterInstance(instance).As(serviceType);
        if (!string.IsNullOrEmpty(name))
        {
            registration.Named(name, serviceType);
        }
        registration.SingleInstance();
        return this;
    }

    /// <summary>
    /// Applies the lifetime to registration
    /// </summary>
    /// <typeparam name="TLimit">Registration limit type</typeparam>
    /// <typeparam name="TActivatorData">Activator data type</typeparam>
    /// <typeparam name="TRegistrationStyle">Registration style</typeparam>
    /// <param name="registration">Registration builder</param>
    /// <param name="lifetime">Service lifetime</param>
    private void ApplyLifetime<TLimit, TActivatorData, TRegistrationStyle>(
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration,
        ServiceLifetime lifetime)
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