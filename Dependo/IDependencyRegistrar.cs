namespace Dependo;

/// <summary>
/// Interface for dependency registrars
/// </summary>
public interface IDependencyRegistrar
{
    /// <summary>
    /// Register services and interfaces
    /// </summary>
    /// <param name="builder">Container builder abstraction</param>
    /// <param name="typeFinder">Type finder to help registration</param>
    void Register(IContainerBuilder builder, ITypeFinder typeFinder);

    /// <summary>
    /// Gets order of this dependency registrar implementation
    /// </summary>
    int Order { get; }
}

/// <summary>
/// Adapter interface for dependency registrars (to use DI framework-specific features)
/// </summary>
public interface IDependencyRegistrarAdapter : IDependencyRegistrar;