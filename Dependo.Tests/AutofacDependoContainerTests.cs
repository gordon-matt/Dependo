using Autofac;
using Dependo.Autofac;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Dependo.Tests;

public class AutofacDependoContainerTests : DependoContainerTestsBase<AutofacDependoContainer, AutofacContainerBuilder>
{
    private readonly ContainerBuilder autofacContainerBuilder = new();
    private readonly AutofacContainerBuilder containerBuilder;

    public AutofacDependoContainerTests()
    {
        containerBuilder = new AutofacContainerBuilder(autofacContainerBuilder);
    }

    protected override AutofacContainerBuilder ContainerBuilder => containerBuilder;

    protected override AutofacDependoContainer ConfigureDependoContainer(Action registerServices)
    {
        registerServices();
        var dependoContainer = new AutofacDependoContainer();
        dependoContainer.ConfigureServices(autofacContainerBuilder, new Mock<IConfigurationRoot>().Object);
        return dependoContainer;
    }
}