using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.Autofac;

public class DependableAutofacServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
{
    private readonly Action<ContainerBuilder> configurationAction;
    private IServiceCollection services;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutofacServiceProviderFactory"/> class.
    /// </summary>
    /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the conatiner.</param>
    public DependableAutofacServiceProviderFactory(Action<ContainerBuilder> configurationAction = null)
    {
        this.configurationAction = configurationAction ?? (builder => { });
    }

    /// <summary>
    /// Creates a container builder from an <see cref="IServiceCollection" />.
    /// </summary>
    /// <param name="services">The collection of services.</param>
    /// <returns>A container builder that can be used to create an <see cref="IServiceProvider" />.</returns>
    public ContainerBuilder CreateBuilder(IServiceCollection services)
    {
        this.services = services; // To use in CreateServiceProvider()

        var builder = new ContainerBuilder();
        builder.Populate(services);
        configurationAction(builder);
        return builder;
    }

    /// <summary>
    /// Creates an <see cref="IServiceProvider" /> from the container builder.
    /// </summary>
    /// <param name="containerBuilder">The container builder.</param>
    /// <returns>An <see cref="IServiceProvider" />.</returns>
    public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
    {
        if (containerBuilder == null)
        {
            throw new ArgumentNullException(nameof(containerBuilder));
        }

        //set base application path
        var provider = services.BuildServiceProvider();
        var configuration = provider.GetService<IConfigurationRoot>();

        var engine = new AutofacEngine();
        var serviceProvider = engine.ConfigureServices(containerBuilder, configuration);
        EngineContext.Create(engine);

        return serviceProvider; // AutofacServiceProvider
    }
}