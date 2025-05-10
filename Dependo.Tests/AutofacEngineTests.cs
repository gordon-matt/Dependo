using Autofac;
using Autofac.Core.Registration;
using Dependo.Autofac;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Dependo.Tests;

public class AutofacEngineTests
{
    private readonly IConfigurationRoot configuration = new Mock<IConfigurationRoot>().Object;
    private readonly ContainerBuilder containerBuilder = new();
    private readonly AutofacEngine engine = new AutofacEngine();

    [Fact]
    public void Resolve_RegisteredType_ReturnsInstance()
    {
        // Arrange
        containerBuilder.RegisterType<TestService>().As<ITestService>().SingleInstance();

        // Configure the engine with our container
        engine.ConfigureServices(containerBuilder, configuration);

        // Act
        var service = engine.Resolve<ITestService>();

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void Resolve_UnregisteredType_ThrowsException()
    {
        // Configure the engine with our container
        engine.ConfigureServices(containerBuilder, configuration);

        // Act & Assert
        Assert.Throws<ComponentNotRegisteredException>(() =>
            engine.Resolve<ITestService>());
    }

    [Fact]
    public void Resolve_WithTypeParameter_ReturnsInstance()
    {
        // Arrange
        containerBuilder.RegisterType<TestService>().As<ITestService>().SingleInstance();

        // Configure the engine with our container
        engine.ConfigureServices(containerBuilder, configuration);

        // Act
        var service = engine.Resolve(typeof(ITestService));

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void ResolveNamed_RegisteredNamedType_ReturnsInstance()
    {
        // Arrange
        containerBuilder.RegisterType<TestService>().Named<ITestService>("test-service").SingleInstance();

        // Configure the engine with our container
        engine.ConfigureServices(containerBuilder, configuration);

        // Act
        var service = engine.ResolveNamed<ITestService>("test-service");

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void ResolveNamed_UnregisteredNamedType_ThrowsException()
    {
        // Arrange
        engine.ConfigureServices(containerBuilder, configuration);

        // Act & Assert
        Assert.Throws<ComponentNotRegisteredException>(() =>
            engine.ResolveNamed<ITestService>("test-service"));
    }

    [Fact]
    public void ResolveAllNamed_MultipleNamedInstances_ReturnsAllInstances()
    {
        // Arrange
        containerBuilder.RegisterType<TestService>().Named<ITestService>("test-services").SingleInstance();
        containerBuilder.RegisterType<AnotherTestService>().Named<ITestService>("test-services").SingleInstance();

        // Configure the engine with our container
        engine.ConfigureServices(containerBuilder, configuration);

        // Act
        var services = engine.ResolveAllNamed<ITestService>("test-services").ToList();

        // Assert
        Assert.Equal(2, services.Count);
        Assert.Contains(services, s => s.GetType() == typeof(TestService));
        Assert.Contains(services, s => s.GetType() == typeof(AnotherTestService));
    }

    [Fact]
    public void ResolveUnregistered_UnregisteredType_CreatesInstance()
    {
        // Arrange
        engine.ConfigureServices(containerBuilder, configuration);

        // Act
        var service = engine.ResolveUnregistered(typeof(ConcreteService));

        // Assert
        Assert.NotNull(service);
        Assert.IsType<ConcreteService>(service);
    }

    [Fact]
    public void TryResolveWithType_RegisteredType_ReturnsTrue()
    {
        // Arrange
        containerBuilder.RegisterType<TestService>().As<ITestService>().SingleInstance();

        // Configure the engine with our container
        engine.ConfigureServices(containerBuilder, configuration);

        // Act
        bool resolved = engine.TryResolve(typeof(ITestService), out var service);

        // Assert
        Assert.True(resolved);
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void TryResolveWithType_UnregisteredType_ReturnsFalse()
    {
        // Configure the engine with our container
        engine.ConfigureServices(containerBuilder, configuration);

        // Act
        bool resolved = engine.TryResolve(typeof(ITestService), out var service);

        // Assert
        Assert.False(resolved);
        Assert.Null(service);
    }

    [Fact]
    public void ResolveAll_MultipleRegisteredInstances_ReturnsAllInstances()
    {
        // Arrange
        containerBuilder.RegisterType<TestService>().As<ITestService>().SingleInstance();
        containerBuilder.RegisterType<AnotherTestService>().As<ITestService>().SingleInstance();

        // Configure the engine with our container
        engine.ConfigureServices(containerBuilder, configuration);

        // Act
        var services = engine.ResolveAll<ITestService>().ToList();

        // Assert
        Assert.Equal(2, services.Count);
        Assert.Contains(services, s => s.GetType() == typeof(TestService));
        Assert.Contains(services, s => s.GetType() == typeof(AnotherTestService));
    }

    [Fact]
    public void TryResolve_RegisteredType_ReturnsTrue()
    {
        // Arrange
        containerBuilder.RegisterType<TestService>().As<ITestService>().SingleInstance();

        // Configure the engine with our container
        engine.ConfigureServices(containerBuilder, configuration);

        // Act
        bool resolved = engine.TryResolve<ITestService>(out var service);

        // Assert
        Assert.True(resolved);
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void TryResolve_UnregisteredType_ReturnsFalse()
    {
        // Configure the engine with our container
        engine.ConfigureServices(containerBuilder, configuration);

        // Act
        bool resolved = engine.TryResolve<ITestService>(out var service);

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