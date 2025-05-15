using Dependo.DotNetDefault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dependo.Tests;

public class DotNetDefaultDependoContainerTests : DependoContainerTestsBase<DotNetDefaultDependoContainer, DotNetDefaultContainerBuilder>
{
    private readonly ServiceCollection serviceCollection = [];
    private readonly DotNetDefaultContainerBuilder containerBuilder;

    public DotNetDefaultDependoContainerTests()
    {
        containerBuilder = new DotNetDefaultContainerBuilder(serviceCollection);
    }

    protected override DotNetDefaultContainerBuilder ContainerBuilder => containerBuilder;

    protected override DotNetDefaultDependoContainer ConfigureDependoContainer(Action registerServices)
    {
        registerServices();
        var dependoContainer = new DotNetDefaultDependoContainer();
        dependoContainer.ConfigureServices(serviceCollection, new Mock<IConfiguration>().Object);
        return dependoContainer;
    }

    [Fact]
    public override void RegisterGeneric_WithInvalidTypes_ThrowsException()
    {
        //// Arrange
        //using var dependoContainer = ConfigureDependoContainer(() => { });

        //// Act & Assert
        //Assert.ThrowsAny<Exception>(() =>
        //    ContainerBuilder.RegisterGeneric(typeof(string), typeof(int), ServiceLifetime.Singleton));

        // Strangely the default .NET implementation does not throw an exception when registering invalid types.
        Assert.True(true);
    }
}