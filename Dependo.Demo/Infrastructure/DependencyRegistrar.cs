using Autofac;
using Dependo.Autofac;
using Dependo.Demo.Services;

namespace Dependo.Demo.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 1;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<HelloWorldService>().As<IHelloWorldService>();
        }
    }
}