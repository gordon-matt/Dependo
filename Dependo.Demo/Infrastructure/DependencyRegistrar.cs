using Dependo.Demo.Services;

namespace Dependo.Demo.Infrastructure;

public class DependencyRegistrar : IDependencyRegistrar
{
    public int Order => 1;

    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
        // Basic service registration
        builder.Register<IHelloWorldService, HelloWorldService>();

        // Registration with lifetime
        builder.Register<IExampleService, ExampleService>(ServiceLifetime.Singleton);

        // More examples of framework-agnostic registrations
        builder.RegisterSelf<ConfigurationService>(ServiceLifetime.Singleton);

        // Find and register all services that implement a specific interface
        foreach (var type in typeFinder.FindClassesOfType<IAutoRegisterService>())
        {
            var serviceType = type.GetInterfaces().FirstOrDefault(i => i != typeof(IAutoRegisterService));
            if (serviceType != null)
            {
                builder.Register(serviceType, type);
            }
        }
    }
}