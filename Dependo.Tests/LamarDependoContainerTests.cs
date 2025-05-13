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
        dependoContainer.ConfigureServices(serviceRegistry, new Mock<IConfigurationRoot>().Object);
        return dependoContainer;
    }

    [Fact]
    public override void ResolveAllNamed_MultipleNamedInstances_ReturnsAllInstances()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
        {
            containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton, "test-services");
            containerBuilder.Register<ITestService, AnotherTestService>(ServiceLifetime.Singleton, "test-services");
        });

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            dependoContainer.ResolveAllNamed<ITestService>("test-services"));
    }
}