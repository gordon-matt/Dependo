using Autofac;
using Autofac.Core.Registration;
using Dependo.Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dependo.Tests;

public class AutofacDependoContainerTests
{
    private readonly ContainerBuilder autofacContainerBuilder = new();
    private readonly IContainerBuilder containerBuilder;

    public AutofacDependoContainerTests()
    {
        containerBuilder = new AutofacContainerBuilder(autofacContainerBuilder);
    }

    public AutofacDependoContainer ConfigureDependoContainer(Action registerServices)
    {
        registerServices();
        var dependoContainer = new AutofacDependoContainer();
        dependoContainer.ConfigureServices(autofacContainerBuilder, new Mock<IConfigurationRoot>().Object);
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
        Assert.Throws<ComponentNotRegisteredException>(() =>
            dependoContainer.Resolve<ITestService>());
    }

    [Fact]
    public void Resolve_WithTypeParameter_ReturnsInstance()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() => containerBuilder.Register(typeof(ITestService), typeof(TestService), ServiceLifetime.Singleton));

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
        using var dependoContainer = ConfigureDependoContainer(() => autofacContainerBuilder.RegisterType<TestService>().Named<ITestService>("test-service").SingleInstance());

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
        Assert.Throws<ComponentNotRegisteredException>(() =>
            dependoContainer.ResolveNamed<ITestService>("test-service"));
    }

    [Fact]
    public void ResolveAllNamed_MultipleNamedInstances_ReturnsAllInstances()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
        {
            autofacContainerBuilder.RegisterType<TestService>().Named<ITestService>("test-services").SingleInstance();
            autofacContainerBuilder.RegisterType<AnotherTestService>().Named<ITestService>("test-services").SingleInstance();
        });

        // Act
        var services = dependoContainer.ResolveAllNamed<ITestService>("test-services").ToList();

        // Assert
        Assert.Equal(2, services.Count);
        Assert.Contains(services, s => s.GetType() == typeof(TestService));
        Assert.Contains(services, s => s.GetType() == typeof(AnotherTestService));
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