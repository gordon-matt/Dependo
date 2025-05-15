using Dependo.Lamar;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dependo.Tests;

public class LamarDependoContainerTests : DependoContainerTestsBase<LamarDependoContainer, LamarContainerBuilder>
{
    private readonly ServiceRegistry serviceRegistry = [];
    private readonly LamarContainerBuilder containerBuilder;

    public LamarDependoContainerTests()
    {
        containerBuilder = new LamarContainerBuilder(serviceRegistry);
    }

    protected override LamarContainerBuilder ContainerBuilder => containerBuilder;

    protected override LamarDependoContainer ConfigureDependoContainer(Action registerServices)
    {
        registerServices();
        var dependoContainer = new LamarDependoContainer();
        dependoContainer.ConfigureServices(serviceRegistry, new Mock<IConfiguration>().Object);
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
}