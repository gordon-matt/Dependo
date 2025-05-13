
using Dependo.LightInject;
using LightInject;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dependo.Tests;

public class LightInjectDependoContainerTests
{
    private readonly ServiceContainer serviceContainer = new();
    private readonly IContainerBuilder containerBuilder;

    public LightInjectDependoContainerTests()
    {
        containerBuilder = new LightInjectContainerBuilder(serviceContainer);
    }

    public LightInjectDependoContainer ConfigureDependoContainer(Action registerServices)
    {
        registerServices();
        var dependoContainer = new LightInjectDependoContainer();
        dependoContainer.ConfigureServices(serviceContainer, new Mock<IConfigurationRoot>().Object);
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
        Assert.Throws<InvalidOperationException>(() => // Seems like a bug in LightInject.. since ResolveNamed correctly throws LightInjectMissingRegistrationException
            dependoContainer.Resolve<ITestService>());

        //Assert.Throws<LightInjectMissingRegistrationException>(() =>
        //    dependoContainer.Resolve<ITestService>());
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
        Assert.Throws<InvalidOperationException>(() =>
            dependoContainer.ResolveNamed<ITestService>("test-service"));
    }

    [Fact]
    public void ResolveAllNamed_MultipleNamedInstances_ReturnsAllInstances()
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

    // Note: LightInject does not support multiple registrations of type same type - it overrides them with whatever the last one registered was
    // TODO: Consider changing this unit test to use different service types.
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
        Assert.Single(services);
        //Assert.Equal(2, services.Count);
        //Assert.Contains(services, s => s.GetType() == typeof(TestService));
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