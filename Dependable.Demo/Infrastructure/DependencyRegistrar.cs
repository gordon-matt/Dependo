using Autofac;
using Dependable.Autofac;
using Dependable.Demo.Services;

namespace Dependable.Demo.Infrastructure
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