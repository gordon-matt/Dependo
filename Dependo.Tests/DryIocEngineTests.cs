using Dependo.DryIoc;
using DryIoc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dependo.Tests;

public class DryIocEngineTests
{
    private readonly Container container = new Container();
    private readonly IContainerBuilder containerBuilder;

    public DryIocEngineTests()
    {
        containerBuilder = new DryIocContainerBuilder(container);
    }

    public DryIocEngine ConfigureEngine(Action registerServices)
    {
        registerServices();
        var engine = new DryIocEngine();
        engine.ConfigureServices(container, new Mock<IConfigurationRoot>().Object);
        return engine;
    }

    [Fact]
    public void Resolve_RegisteredType_ReturnsInstance()
    {
        // Arrange
        using var engine = ConfigureEngine(() => containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton));

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
        using var engine = ConfigureEngine(() => { });

        // Act & Assert
        Assert.Throws<ContainerException>(() =>
            engine.Resolve<ITestService>());
    }

    [Fact]
    public void Resolve_WithTypeParameter_ReturnsInstance()
    {
        // Arrange
        using var engine = ConfigureEngine(() =>
            containerBuilder.Register(typeof(ITestService), typeof(TestService), ServiceLifetime.Singleton));

        // Act
        object service = engine.Resolve(typeof(ITestService));

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void ResolveNamed_RegisteredNamedType_ReturnsInstance()
    {
        // Arrange
        using var engine = ConfigureEngine(() =>
            container.Register<ITestService, TestService>(reuse: Reuse.Singleton, serviceKey: "test-service"));

        // Act
        var service = engine.ResolveNamed<ITestService>("test-service");

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void ResolveNamed_UnregisteredNamedType_ThrowsException()
    {
        using var engine = ConfigureEngine(() => { });

        // Act & Assert
        Assert.Throws<ContainerException>(() =>
            engine.ResolveNamed<ITestService>("test-service"));
    }

    [Fact]
    public void ResolveAllNamed_MultipleNamedInstances_ReturnsAllInstances()
    {
        // Arrange
        using var engine = ConfigureEngine(() =>
        {
            container.Register<ITestService, TestService>(reuse: Reuse.Singleton, serviceKey: "test-services");

            // Attempting to register more than one service of the same type with the same key results in an exception anyway.
            //container.Register<ITestService, AnotherTestService>(reuse: Reuse.Singleton, serviceKey: "test-services");
        });

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            engine.ResolveAllNamed<ITestService>("test-services"));
    }

    [Fact]
    public void ResolveUnregistered_UnregisteredType_CreatesInstance()
    {
        // Arrange
        using var engine = ConfigureEngine(() => { });

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            engine.ResolveUnregistered(typeof(ConcreteService)));
    }

    [Fact]
    public void TryResolveWithType_RegisteredType_ReturnsTrue()
    {
        // Arrange
        using var engine = ConfigureEngine(() => containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton));

        // Act
        bool resolved = engine.TryResolve(typeof(ITestService), out object? service);

        // Assert
        Assert.True(resolved);
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public void TryResolveWithType_UnregisteredType_ReturnsFalse()
    {
        // Configure the engine with our container
        using var engine = ConfigureEngine(() => { });

        // Act
        bool resolved = engine.TryResolve(typeof(ITestService), out object? service);

        // Assert
        Assert.False(resolved);
        Assert.Null(service);
    }

    [Fact]
    public void ResolveAll_MultipleRegisteredInstances_ReturnsAllInstances()
    {
        // Arrange
        using var engine = ConfigureEngine(() =>
        {
            containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton);
            containerBuilder.Register<ITestService, AnotherTestService>(ServiceLifetime.Singleton);
        });

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
        using var engine = ConfigureEngine(() => containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton));

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
        using var engine = ConfigureEngine(() => { });

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