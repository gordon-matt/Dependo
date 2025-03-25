using Autofac;

namespace Dependo.Autofac;

public interface IDependencyRegistrar
{
    void Register(ContainerBuilder builder, ITypeFinder typeFinder);

    int Order { get; }
}