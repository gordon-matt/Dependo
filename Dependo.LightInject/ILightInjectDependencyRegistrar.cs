using LightInject;

namespace Dependo.LightInject;

/// <summary>
/// Interface for LightInject dependency registrars.
/// Use this instead of IDependencyRegistrar if you want to take advantage of LightInject-specific features.
/// </summary>
/// <remarks>
/// This interface extends the framework-agnostic IDependencyRegistrar
/// for backward compatibility
/// </remarks>
public interface ILightInjectDependencyRegistrar : IDependencyRegistrarAdapter
{
    /// <summary>
    /// Register services and interfaces with LightInject
    /// </summary>
    /// <param name="container">Service container</param>
    /// <param name="typeFinder">Type finder to help registration</param>
    void Register(IServiceContainer container, ITypeFinder typeFinder);
} 