using Microsoft.Extensions.DependencyInjection;

namespace Dependo.Tests;

public abstract class DependoContainerTestsBase<TContainer, TContainerBuilder>
    where TContainer : IDependoContainer
    where TContainerBuilder : IContainerBuilder
{
    protected abstract TContainerBuilder ContainerBuilder { get; }

    protected abstract TContainer ConfigureDependoContainer(Action registerServices);

    [Fact]
    public virtual void Resolve_RegisteredType_ReturnsInstance()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() => ContainerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton));

        // Act
        var service = dependoContainer.Resolve<ITestService>();

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public virtual void Resolve_UnregisteredType_ThrowsException()
    {
        // Configure the dependoContainer with our container
        using var dependoContainer = ConfigureDependoContainer(() => { });

        // Act & Assert
        Assert.ThrowsAny<Exception>(() =>
            dependoContainer.Resolve<ITestService>());
    }

    [Fact]
    public virtual void Resolve_WithTypeParameter_ReturnsInstance()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() => ContainerBuilder.Register(typeof(ITestService), typeof(TestService), ServiceLifetime.Singleton));

        // Act
        object service = dependoContainer.Resolve(typeof(ITestService));

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public virtual void ResolveNamed_RegisteredNamedType_ReturnsInstance()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
            ContainerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton, "test-service"));

        // Act
        var service = dependoContainer.ResolveNamed<ITestService>("test-service");

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public virtual void ResolveNamed_UnregisteredNamedType_ThrowsException()
    {
        using var dependoContainer = ConfigureDependoContainer(() => { });
        // Act & Assert
        Assert.ThrowsAny<Exception>(() =>
            dependoContainer.ResolveNamed<ITestService>("test-service"));
    }

    [Fact]
    public virtual void ResolveAllNamed_MultipleNamedInstances_ReturnsAllInstances()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
        {
            ContainerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton, "test-services");
            ContainerBuilder.Register<ITestService, AnotherTestService>(ServiceLifetime.Singleton, "test-services");
        });

        // Act
        var services = dependoContainer.ResolveAllNamed<ITestService>("test-services").ToList();

        // Assert
        Assert.Equal(2, services.Count);
        Assert.Contains(services, s => s.GetType() == typeof(TestService));
        Assert.Contains(services, s => s.GetType() == typeof(AnotherTestService));
    }

    [Fact]
    public virtual void Register_WithName_RegistersNamedService()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
            ContainerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton, "named-service"));

        // Act
        var service = dependoContainer.ResolveNamed<ITestService>("named-service");

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public virtual void RegisterSelf_WithName_RegistersNamedService()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
            ContainerBuilder.RegisterSelf<TestService>(ServiceLifetime.Singleton, "named-service"));

        // Act
        var service = dependoContainer.ResolveNamed<TestService>("named-service");

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public virtual void RegisterInstance_WithName_RegistersNamedInstance()
    {
        // Arrange
        var instance = new TestService();
        using var dependoContainer = ConfigureDependoContainer(() =>
            ContainerBuilder.RegisterInstance<ITestService>(instance, "named-instance"));

        // Act
        var service = dependoContainer.ResolveNamed<ITestService>("named-instance");

        // Assert
        Assert.NotNull(service);
        Assert.Same(instance, service);
    }

    [Fact]
    public virtual void ResolveUnregistered_UnregisteredType_CreatesInstance()
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
    public virtual void TryResolveWithType_RegisteredType_ReturnsTrue()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() => ContainerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton));

        // Act
        bool resolved = dependoContainer.TryResolve(typeof(ITestService), out object? service);

        // Assert
        Assert.True(resolved);
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public virtual void TryResolveWithType_UnregisteredType_ReturnsFalse()
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
    public virtual void ResolveAll_MultipleRegisteredInstances_ReturnsAllInstances()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
        {
            ContainerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton);
            ContainerBuilder.Register<ITestService, AnotherTestService>(ServiceLifetime.Singleton);
        });

        // Act
        var services = dependoContainer.ResolveAll<ITestService>().ToList();

        // Assert
        Assert.Equal(2, services.Count);
        Assert.Contains(services, s => s.GetType() == typeof(TestService));
        Assert.Contains(services, s => s.GetType() == typeof(AnotherTestService));
    }

    [Fact]
    public virtual void TryResolve_RegisteredType_ReturnsTrue()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() => ContainerBuilder.Register<ITestService, TestService>(ServiceLifetime.Singleton));

        // Act
        bool resolved = dependoContainer.TryResolve<ITestService>(out var service);

        // Assert
        Assert.True(resolved);
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public virtual void TryResolve_UnregisteredType_ReturnsFalse()
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