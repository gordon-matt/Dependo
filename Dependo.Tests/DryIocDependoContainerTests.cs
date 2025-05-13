using Dependo.DryIoc;
using DryIoc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dependo.Tests;

public class DryIocDependoContainerTests : DependoContainerTestsBase<DryIocDependoContainer, DryIocContainerBuilder>
{
    private readonly Container container = new();
    private readonly DryIocContainerBuilder containerBuilder;

    public DryIocDependoContainerTests()
    {
        containerBuilder = new DryIocContainerBuilder(container);
    }

    protected override DryIocContainerBuilder ContainerBuilder => containerBuilder;

    protected override DryIocDependoContainer ConfigureDependoContainer(Action registerServices)
    {
        registerServices();
        var dependoContainer = new DryIocDependoContainer();
        dependoContainer.ConfigureServices(container, new Mock<IConfigurationRoot>().Object);
        return dependoContainer;
    }

    [Fact]
    public override void ResolveAllNamed_MultipleNamedInstances_ReturnsAllInstances()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
        {
            containerBuilder.RegisterNamed<ITestService, TestService>("test-services", ServiceLifetime.Singleton);

            // Attempting to register more than one service of the same type with the same key results in an exception anyway.
            //containerBuilder.Register<ITestService, AnotherTestService>(ServiceLifetime.Singleton, "test-services");
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

            // Attempting to register more than one service of the same type with the same key results in an exception anyway.
            //containerBuilder.RegisterNamed<ITestService, AnotherTestService>("test-services", ServiceLifetime.Singleton);
        });

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            dependoContainer.ResolveAllKeyed<ITestService>("test-services"));
    }
}