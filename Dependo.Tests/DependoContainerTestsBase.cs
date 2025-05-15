using Microsoft.Extensions.DependencyInjection;

namespace Dependo.Tests;

public abstract class DependoContainerTestsBase<TContainer, TContainerBuilder>
    where TContainer : IDependoContainer
    where TContainerBuilder : IContainerBuilder
{
    // Test interfaces and classes
    public interface ITestService { }

    // Add generic test interfaces and classes
    public interface IGenericService<T> { }
    public class GenericService<T> : IGenericService<T> { }
    public class StringGenericService : IGenericService<string> { }
    public class IntGenericService : IGenericService<int> { }

    protected abstract TContainerBuilder ContainerBuilder { get; }

    [Fact]
    public virtual void Register_WithKey_RegistersKeyedService()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
            ContainerBuilder.RegisterKeyed<ITestService, TestService>("keyed-service", ServiceLifetime.Singleton));

        // Act
        var service = dependoContainer.ResolveKeyed<ITestService>("keyed-service");

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public virtual void Register_WithName_RegistersNamedService()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
            ContainerBuilder.RegisterNamed<ITestService, TestService>("named-service", ServiceLifetime.Singleton));

        // Act
        var service = dependoContainer.ResolveNamed<ITestService>("named-service");

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public virtual void RegisterInstance_WithKey_RegistersKeyedInstance()
    {
        // Arrange
        var instance = new TestService();
        using var dependoContainer = ConfigureDependoContainer(() =>
            ContainerBuilder.RegisterInstanceKeyed<ITestService>(instance, "keyed-instance"));

        // Act
        var service = dependoContainer.ResolveKeyed<ITestService>("keyed-instance");

        // Assert
        Assert.NotNull(service);
        Assert.Same(instance, service);
    }

    [Fact]
    public virtual void RegisterInstance_WithName_RegistersNamedInstance()
    {
        // Arrange
        var instance = new TestService();
        using var dependoContainer = ConfigureDependoContainer(() =>
            ContainerBuilder.RegisterInstanceNamed<ITestService>(instance, "named-instance"));

        // Act
        var service = dependoContainer.ResolveNamed<ITestService>("named-instance");

        // Assert
        Assert.NotNull(service);
        Assert.Same(instance, service);
    }

    [Fact]
    public virtual void RegisterSelf_WithKey_RegistersKeyedService()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
            ContainerBuilder.RegisterSelfKeyed<TestService>("keyed-service", ServiceLifetime.Singleton));

        // Act
        var service = dependoContainer.ResolveKeyed<TestService>("keyed-service");

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public virtual void RegisterSelf_WithName_RegistersNamedService()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
            ContainerBuilder.RegisterSelfNamed<TestService>("named-service", ServiceLifetime.Singleton));

        // Act
        var service = dependoContainer.ResolveNamed<TestService>("named-service");

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

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
    public virtual void ResolveAllKeyed_MultipleKeyedInstances_ReturnsAllInstances()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
        {
            ContainerBuilder.RegisterKeyed<ITestService, TestService>("test-services", ServiceLifetime.Singleton);
            ContainerBuilder.RegisterKeyed<ITestService, AnotherTestService>("test-services", ServiceLifetime.Singleton);
        });

        // Act
        var services = dependoContainer.ResolveAllKeyed<ITestService>("test-services").ToList();

        // Assert
        Assert.Equal(2, services.Count);
        Assert.Contains(services, s => s.GetType() == typeof(TestService));
        Assert.Contains(services, s => s.GetType() == typeof(AnotherTestService));
    }

    [Fact]
    public virtual void ResolveAllNamed_MultipleNamedInstances_ReturnsAllInstances()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
        {
            ContainerBuilder.RegisterNamed<ITestService, TestService>("test-services", ServiceLifetime.Singleton);
            ContainerBuilder.RegisterNamed<ITestService, AnotherTestService>("test-services", ServiceLifetime.Singleton);
        });

        // Act
        var services = dependoContainer.ResolveAllNamed<ITestService>("test-services").ToList();

        // Assert
        Assert.Equal(2, services.Count);
        Assert.Contains(services, s => s.GetType() == typeof(TestService));
        Assert.Contains(services, s => s.GetType() == typeof(AnotherTestService));
    }

    [Fact]
    public virtual void ResolveKeyed_RegisteredKeyedType_ReturnsInstance()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
            ContainerBuilder.RegisterKeyed<ITestService, TestService>("test-service", ServiceLifetime.Singleton));

        // Act
        var service = dependoContainer.ResolveKeyed<ITestService>("test-service");

        // Assert
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }

    [Fact]
    public virtual void ResolveKeyed_UnregisteredKeyedType_ThrowsException()
    {
        using var dependoContainer = ConfigureDependoContainer(() => { });
        // Act & Assert
        Assert.ThrowsAny<Exception>(() =>
            dependoContainer.ResolveKeyed<ITestService>("test-service"));
    }

    [Fact]
    public virtual void ResolveNamed_RegisteredNamedType_ReturnsInstance()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
            ContainerBuilder.RegisterNamed<ITestService, TestService>("test-service", ServiceLifetime.Singleton));

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
    public virtual void RegisterGeneric_WithOpenGenericTypes_RegistersService()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
            ContainerBuilder.RegisterGeneric(typeof(IGenericService<>), typeof(GenericService<>), ServiceLifetime.Singleton));

        // Act
        var stringService = dependoContainer.Resolve<IGenericService<string>>();
        var intService = dependoContainer.Resolve<IGenericService<int>>();

        // Assert
        Assert.NotNull(stringService);
        Assert.NotNull(intService);
        Assert.IsType<GenericService<string>>(stringService);
        Assert.IsType<GenericService<int>>(intService);
    }

    [Fact]
    public virtual void RegisterGeneric_WithClosedGenericTypes_RegistersService()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
        {
            ContainerBuilder.RegisterGeneric(typeof(IGenericService<>), typeof(GenericService<>), ServiceLifetime.Singleton);
            ContainerBuilder.Register<IGenericService<string>, StringGenericService>(ServiceLifetime.Singleton);
        });

        // Act
        var stringService = dependoContainer.Resolve<IGenericService<string>>();
        var intService = dependoContainer.Resolve<IGenericService<int>>();

        // Assert
        Assert.NotNull(stringService);
        Assert.NotNull(intService);
        Assert.IsType<StringGenericService>(stringService);
        Assert.IsType<GenericService<int>>(intService);
    }

    [Fact]
    public virtual void RegisterGeneric_WithDifferentLifetimes_RespectsLifetime()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() =>
            ContainerBuilder.RegisterGeneric(typeof(IGenericService<>), typeof(GenericService<>), ServiceLifetime.Transient));

        // Act
        var stringService1 = dependoContainer.Resolve<IGenericService<string>>();
        var stringService2 = dependoContainer.Resolve<IGenericService<string>>();

        // Assert
        Assert.NotNull(stringService1);
        Assert.NotNull(stringService2);
        Assert.NotSame(stringService1, stringService2);
    }

    [Fact]
    public virtual void RegisterGeneric_WithInvalidTypes_ThrowsException()
    {
        // Arrange
        using var dependoContainer = ConfigureDependoContainer(() => { });

        // Act & Assert
        Assert.ThrowsAny<Exception>(() =>
            ContainerBuilder.RegisterGeneric(typeof(string), typeof(int), ServiceLifetime.Singleton));
    }

    protected abstract TContainer ConfigureDependoContainer(Action registerServices);

    public class AnotherTestService : ITestService { }

    public class ConcreteService { }

    public class TestService : ITestService { }
}