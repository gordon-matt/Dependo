using Microsoft.Extensions.DependencyInjection;

namespace Dependo.DotNetDefault;

/// <summary>
/// Interface for Autofac dependency registrars.
/// Use this instead of IDependencyRegistrar if you want to take advantage of Autofac-specific features.
/// </summary>
/// <remarks>
/// This interface extends the framework-agnostic IDependencyRegistrar
/// for backward compatibility
/// </remarks>
public interface IDotNetDefaultDependencyRegistrar : IDependencyRegistrarAdapter
{
    /// <summary>
    /// Register services and interfaces with Autofac
    /// </summary>
    /// <param name="builder">Container builder</param>
    /// <param name="typeFinder">Type finder to help registration</param>
    void Register(IServiceCollection services, ITypeFinder typeFinder);
}