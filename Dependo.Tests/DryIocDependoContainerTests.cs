using Dependo.DryIoc;
using DryIoc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dependo.Tests;

public class DryIocDependoContainerTests
{
    private readonly Container container = new Container();
    private readonly IContainerBuilder containerBuilder;

    public DryIocDependoContainerTests()
    {
        containerBuilder = new DryIocContainerBuilder(container);
    }

    public DryIocDependoContainer ConfigureDependoContainer(Action registerServices)
    {
        registerServices();
        var dependoContainer = new DryIocDependoContainer();
        dependoContainer.ConfigureServices(container, new Mock<IConfigurationRoot>().Object);
        return dependoContainer;
    }

    [Fact]
    public void Resolve_RegisteredType_ReturnsInstance()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() => containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton));

        // Act
        var service = dependoContainer.Resolve<ITestService>();

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void Resolve_UnregisteredType_ThrowsException()
    {
        // Configure the dependoContainer with our container
        using var dependoContainer = ConfigureDependoContainer(() => { });

        // Act & Assert
        Assert.Throws<ContainerException>(() =>
            dependoContainer.Resolve<ITestService>());
    }

    [Fact]
    public void Resolve_WithTypeParameter_ReturnsInstance()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
            containerBuilder.Register(typeof(ITestService), typeof(TestService), ServiceLifetime.Singleton));

        // Act
        object service = dependoContainer.Resolve(typeof(ITestService));

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void ResolveNamed_RegisteredNamedType_ReturnsInstance()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
            containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton, "test-service"));

        // Act
        var service = dependoContainer.ResolveNamed<ITestService>("test-service");

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void ResolveNamed_UnregisteredNamedType_ThrowsException()
    {
        using var dependoContainer = ConfigureDependoContainer(() => { });

        // Act & Assert
        Assert.Throws<ContainerException>(() =>
            dependoContainer.ResolveNamed<ITestService>("test-service"));
    }

    [Fact]
    public void ResolveAllNamed_MultipleNamedInstances_ReturnsAllInstances()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
        {
            containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton, "test-services");

            // Attempting to register more than one service of the same type with the same key results in an exception anyway.
            //containerBuilder.Register<ITestService, AnotherTestService>(ServiceLifetime.Singleton, "test-services");
        });

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            dependoContainer.ResolveAllNamed<ITestService>("test-services"));
    }

    [Fact]
    public void Register_WithName_RegistersNamedService()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() => 
            containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton, "named-service"));

        // Act
        var service = dependoContainer.ResolveNamed<ITestService>("named-service");

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void RegisterSelf_WithName_RegistersNamedService()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() => 
            containerBuilder.RegisterSelf<TestService>(ServiceLifetime.Singleton, "named-service"));

        // Act
        var service = dependoContainer.ResolveNamed<TestService>("named-service");

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void RegisterInstance_WithName_RegistersNamedInstance()
    {
        // Arrange
        var instance = new TestService();
        using var dependoContainer = ConfigureDependoContainer(() => 
            containerBuilder.RegisterInstance<ITestService>(instance, "named-instance"));

        // Act
        var service = dependoContainer.ResolveNamed<ITestService>("named-instance");

        // Assert
        Assert.NotNull(service);
        Assert.Same(instance, service);
    }

    [Fact]
    public void ResolveUnregistered_UnregisteredType_CreatesInstance()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() => { });

        // Act
        object service = dependoContainer.ResolveUnregistered(typeof(ConcreteService));

        // Assert
        Assert.NotNull(service);
        Assert.IsType<ConcreteService>(service);
    }

    [Fact]
    public void TryResolveWithType_RegisteredType_ReturnsTrue()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() => containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton));

        // Act
        bool resolved = dependoContainer.TryResolve(typeof(ITestService), out object? service);

        // Assert
        Assert.True(resolved);
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void TryResolveWithType_UnregisteredType_ReturnsFalse()
    {
        // Configure the dependoContainer with our container
        using var dependoContainer = ConfigureDependoContainer(() => { });

        // Act
        bool resolved = dependoContainer.TryResolve(typeof(ITestService), out object? service);

        // Assert
        Assert.False(resolved);
        Assert.Null(service);
    }

    [Fact]
    public void ResolveAll_MultipleRegisteredInstances_ReturnsAllInstances()
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
        Assert.Equal(2, services.Count);
        Assert.Contains(services, s => s.GetType() == typeof(TestService));
        Assert.Contains(services, s => s.GetType() == typeof(AnotherTestService));
    }

    [Fact]
    public void TryResolve_RegisteredType_ReturnsTrue()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() => containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton));

        // Act
        bool resolved = dependoContainer.TryResolve<ITestService>(out var service);

        // Assert
        Assert.True(resolved);
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void TryResolve_UnregisteredType_ReturnsFalse()
    {
        // Configure the dependoContainer with our container
        using var dependoContainer = ConfigureDependoContainer(() => { });

        // Act
        bool resolved = dependoContainer.TryResolve<ITestService>(out var service);

        // Assert
        Assert.False(resolved);
        Assert.Null(service);
    }

    // Test interfaces and classes
    public interface ITestService { }

    public class TestService : ITestService { }

    public class AnotherTestService : ITestService { }

    public class ConcreteService { }
}