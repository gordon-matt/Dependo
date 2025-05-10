using DryIoc;

namespace Dependo.DryIoc;

/// <summary>
/// Interface for DryIoc dependency registrars.
/// Use this instead of IDependencyRegistrar if you want to take advantage of DryIoc-specific features.
/// </summary>
/// <remarks>
/// This interface extends the framework-agnostic IDependencyRegistrar
/// for backward compatibility
/// </remarks>
public interface IDryIocDependencyRegistrar : IDependencyRegistrarAdapter
{
    /// <summary>
    /// Register services and interfaces with DryIoc
    /// </summary>
    /// <param name="container">DryIoc container</param>
    /// <param name="typeFinder">Type finder to help registration</param>
    void Register(IContainer container, ITypeFinder typeFinder);
}