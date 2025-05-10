using Lamar;

namespace Dependo.Lamar;

/// <summary>
/// Interface for Lamar dependency registrars.
/// Use this instead of IDependencyRegistrar if you want to take advantage of Lamar-specific features.
/// </summary>
/// <remarks>
/// This interface extends the framework-agnostic IDependencyRegistrar
/// for backward compatibility
/// </remarks>
public interface ILamarDependencyRegistrar : IDependencyRegistrarAdapter
{
    /// <summary>
    /// Register services and interfaces with Lamar
    /// </summary>
    /// <param name="registry">Service registry</param>
    /// <param name="typeFinder">Type finder to help registration</param>
    void Register(ServiceRegistry registry, ITypeFinder typeFinder);
}