using LightInject;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.LightInject;

public class LightInjectCompositionRoot : ICompositionRoot
{
    private readonly IServiceCollection _services;

    public LightInjectCompositionRoot(IServiceCollection services)
    {
        _services = services;
    }

    public void Compose(IServiceRegistry serviceRegistry)
    {
        // Register all services from the IServiceCollection
        foreach (var service in _services)
        {
            switch (service.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    if (service.ImplementationInstance != null)
                    {
                        serviceRegistry.RegisterInstance(service.ServiceType, service.ImplementationInstance);
                    }
                    else if (service.ImplementationFactory != null)
                    {
                        serviceRegistry.Register(service.ServiceType, factory =>
                            service.ImplementationFactory(factory.GetInstance<IServiceProvider>()),
                            new PerContainerLifetime());
                    }
                    else
                    {
                        serviceRegistry.Register(service.ServiceType, service.ImplementationType ?? service.ServiceType,
                            new PerContainerLifetime());
                    }
                    break;

                case ServiceLifetime.Scoped:
                    if (service.ImplementationFactory != null)
                    {
                        serviceRegistry.Register(service.ServiceType, factory =>
                            service.ImplementationFactory(factory.GetInstance<IServiceProvider>()),
                            new PerScopeLifetime());
                    }
                    else
                    {
                        serviceRegistry.Register(service.ServiceType, service.ImplementationType ?? service.ServiceType,
                            new PerScopeLifetime());
                    }
                    break;

                case ServiceLifetime.Transient:
                    if (service.ImplementationFactory != null)
                    {
                        serviceRegistry.Register(service.ServiceType, factory =>
                            service.ImplementationFactory(factory.GetInstance<IServiceProvider>()));
                    }
                    else
                    {
                        serviceRegistry.Register(service.ServiceType, service.ImplementationType ?? service.ServiceType);
                    }
                    break;
            }
        }
    }
}