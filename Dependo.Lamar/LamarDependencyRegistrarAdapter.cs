using Microsoft.Extensions.Configuration;

namespace Dependo.Lamar;

/// <summary>
/// Adapter class to bridge the Lamar-specific IDependencyRegistrar with the framework-agnostic interface.
/// </summary>
public class LamarDependencyRegistrarAdapter : IDependencyRegistrarAdapter
{
    private readonly ILamarDependencyRegistrar _lamarRegistrar;

    /// <summary>
    /// Initializes a new instance of the LamarDependencyRegistrarAdapter class
    /// </summary>
    /// <param name="lamarRegistrar">Lamar-specific registrar</param>
    public LamarDependencyRegistrarAdapter(ILamarDependencyRegistrar lamarRegistrar)
    {
        _lamarRegistrar = lamarRegistrar;
    }

    /// <inheritdoc/>
    public int Order => _lamarRegistrar.Order;

    /// <inheritdoc/>
    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
        // Extract the Lamar service registry if possible
        if (builder is LamarContainerBuilder lamarBuilder)
        {
            _lamarRegistrar.Register(lamarBuilder.NativeRegistry, typeFinder);
        }
    }

    /// <summary>
    /// Creates an instance of the LamarDependencyRegistrarAdapter from a Type representing an ILamarDependencyRegistrar
    /// </summary>
    /// <param name="lamarRegistrarType">Type that implements ILamarDependencyRegistrar</param>
    /// <returns>A new instance of LamarDependencyRegistrarAdapter</returns>
    /// <exception cref="ArgumentException">Thrown when the provided type doesn't implement ILamarDependencyRegistrar</exception>
    /// <exception cref="MissingMethodException">Thrown when the ILamarDependencyRegistrar type doesn't have a parameterless constructor</exception>
    public static LamarDependencyRegistrarAdapter CreateFromType(Type lamarRegistrarType)
    {
        if (!typeof(ILamarDependencyRegistrar).IsAssignableFrom(lamarRegistrarType))
        {
            throw new ArgumentException($"Type {lamarRegistrarType.Name} must implement ILamarDependencyRegistrar", nameof(lamarRegistrarType));
        }

        // Create an instance of the ILamarDependencyRegistrar
        var lamarRegistrar = (ILamarDependencyRegistrar)Activator.CreateInstance(lamarRegistrarType)!;

        // Create and return the adapter
        return new LamarDependencyRegistrarAdapter(lamarRegistrar);
    }
}