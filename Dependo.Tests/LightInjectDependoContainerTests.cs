using Dependo.LightInject;
using LightInject;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dependo.Tests;

public class LightInjectDependoContainerTests : DependoContainerTestsBase<LightInjectDependoContainer, LightInjectContainerBuilder>
{
    private readonly ServiceContainer serviceContainer = new();
    private readonly LightInjectContainerBuilder containerBuilder;

    public LightInjectDependoContainerTests()
    {
        containerBuilder = new LightInjectContainerBuilder(serviceContainer);
    }

    protected override LightInjectContainerBuilder ContainerBuilder => containerBuilder;

    protected override LightInjectDependoContainer ConfigureDependoContainer(Action registerServices)
    {
        registerServices();
        var dependoContainer = new LightInjectDependoContainer();
        dependoContainer.ConfigureServices(serviceContainer, new Mock<IConfiguration>().Object);
        return dependoContainer;
    }

    [Fact]
    public override void ResolveAllNamed_MultipleNamedInstances_ReturnsAllInstances()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
        {
            containerBuilder.RegisterNamed<ITestService, TestService>("test-services", ServiceLifetime.Singleton);
            containerBuilder.RegisterNamed<ITestService, AnotherTestService>("test-services", ServiceLifetime.Singleton);
        });

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            dependoContainer.ResolveAllNamed<ITestService>("test-services"));
    }

    [Fact]
    public override void ResolveAllKeyed_MultipleKeyedInstances_ReturnsAllInstances()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
        {
            containerBuilder.RegisterNamed<ITestService, TestService>("test-services", ServiceLifetime.Singleton);
            containerBuilder.RegisterNamed<ITestService, AnotherTestService>("test-services", ServiceLifetime.Singleton);
        });

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            dependoContainer.ResolveAllKeyed<ITestService>("test-services"));
    }

    // Note: LightInject does not support multiple registrations of type same type - it overrides them with whatever the last one registered was
    // TODO: Consider changing this unit test to use different service types.
    [Fact]
    public override void ResolveAll_MultipleRegisteredInstances_ReturnsAllInstances()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
        {
            containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton);
            containerBuilder.Register<ITestService, AnotherTestService>(ServiceLifetime.Singleton);
        });

        // Act
        var services = dependoContainer.ResolveAll<ITestService>().ToList();

        // Assert
        Assert.Single(services);
        //Assert.Equal(2, services.Count);
        //Assert.Contains(services, s => s.GetType() == typeof(TestService));
        Assert.Contains(services, s => s.GetType() == typeof(AnotherTestService));
    }
}