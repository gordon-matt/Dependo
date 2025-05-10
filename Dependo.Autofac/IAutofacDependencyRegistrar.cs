using Autofac;

namespace Dependo.Autofac;

/// <summary>
/// Interface for Autofac dependency registrars.
/// Use this instead of IDependencyRegistrar if you want to take advantage of Autofac-specific features.
/// </summary>
/// <remarks>
/// This interface extends the framework-agnostic IDependencyRegistrar
/// for backward compatibility
/// </remarks>
public interface IAutofacDependencyRegistrar : IDependencyRegistrarAdapter
{
    /// <summary>
    /// Register services and interfaces with Autofac
    /// </summary>
    /// <param name="builder">Container builder</param>
    /// <param name="typeFinder">Type finder to help registration</param>
    void Register(ContainerBuilder builder, ITypeFinder typeFinder);
}