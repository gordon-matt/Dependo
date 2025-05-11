using Dependo.DotNetDefault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dependo.Tests;

public class DotNetDefaultDependoContainerTests
{
    private readonly ServiceCollection serviceCollection = [];
    private readonly IContainerBuilder containerBuilder;

    public DotNetDefaultDependoContainerTests()
    {
        containerBuilder = new DotNetDefaultContainerBuilder(serviceCollection);
    }

    public DotNetDefaultDependoContainer ConfigureDependoContainer(Action registerServices)
    {
        registerServices();
        var dependoContainer = new DotNetDefaultDependoContainer();
        dependoContainer.ConfigureServices(serviceCollection, new Mock<IConfigurationRoot>().Object);
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
        Assert.Throws<InvalidOperationException>(() => // Seems like a bug in Lamar.. since ResolveNamed correctly throws LamarMissingRegistrationException
            dependoContainer.Resolve<ITestService>());

        //Assert.Throws<LamarMissingRegistrationException>(() =>
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
        using var dependoContainer = ConfigureDependoContainer(() =>
            Assert.Throws<NotSupportedException>(() =>
                containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton, "test-service"))
            );
    }

    [Fact]
    public void ResolveNamed_UnregisteredNamedType_ThrowsException()
    {
        using var dependoContainer = ConfigureDependoContainer(() => { });

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            dependoContainer.ResolveNamed<ITestService>("test-service"));
    }

    [Fact]
    public void ResolveAllNamed_MultipleNamedInstances_ReturnsAllInstances()
    {
        using var dependoContainer = ConfigureDependoContainer(() =>
            Assert.Throws<NotSupportedException>(() =>
                {
                    containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton, "test-services");
                    containerBuilder.Register<ITestService, AnotherTestService>(ServiceLifetime.Singleton, "test-services");
                }
            ));
    }

    [Fact]
    public void Register_WithName_RegistersNamedService()
    {
        using var dependoContainer = ConfigureDependoContainer(() =>
            Assert.Throws<NotSupportedException>(() =>
                containerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton, "named-service"))
            );
    }

    [Fact]
    public void RegisterSelf_WithName_RegistersNamedService()
    {
        using var dependoContainer = ConfigureDependoContainer(() =>
            Assert.Throws<NotSupportedException>(() =>
                containerBuilder.RegisterSelf<TestService>(ServiceLifetime.Singleton, "named-service"))
            );
    }

    [Fact]
    public void RegisterInstance_WithName_RegistersNamedInstance()
    {
        var instance = new TestService();
        using var dependoContainer = ConfigureDependoContainer(() =>
            Assert.Throws<NotSupportedException>(() =>
                containerBuilder.RegisterInstance<ITestService>(instance, "named-instance"))
            );
    }

    [Fact]
    public void ResolveUnregistered_UnregisteredType_CreatesInstance()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() => { });

        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            dependoContainer.ResolveUnregistered(typeof(ConcreteService)));
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