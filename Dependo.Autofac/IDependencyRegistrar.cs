using Autofac;

namespace Dependo.Autofac;

/// <summary>
/// Interface for dependency registrars
/// </summary>
public interface IDependencyRegistrar
{
    /// <summary>
    /// Register services and interfaces
    /// </summary>
    /// <param name="builder">Container builder</param>
    /// <param name="typeFinder">Type finder to help registration</param>
    void Register(ContainerBuilder builder, ITypeFinder typeFinder);

    /// <summary>
    /// Gets order of this dependency registrar implementation
    /// </summary>
    int Order { get; }
}