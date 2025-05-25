using Microsoft.Extensions.Configuration;

namespace Dependo.LightInject;

/// <summary>
/// Adapter class to bridge the LightInject-specific IDependencyRegistrar with the framework-agnostic interface.
/// </summary>
public class LightInjectDependencyRegistrarAdapter : IDependencyRegistrarAdapter
{
    private readonly ILightInjectDependencyRegistrar _lightInjectRegistrar;

    /// <summary>
    /// Initializes a new instance of the LightInjectDependencyRegistrarAdapter class
    /// </summary>
    /// <param name="lightInjectRegistrar">LightInject-specific registrar</param>
    public LightInjectDependencyRegistrarAdapter(ILightInjectDependencyRegistrar lightInjectRegistrar)
    {
        _lightInjectRegistrar = lightInjectRegistrar;
    }

    /// <inheritdoc/>
    public int Order => _lightInjectRegistrar.Order;

    /// <inheritdoc/>
    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
        // First handle any registrations made via the generic IDependencyRegistrar interface
        _lightInjectRegistrar.Register(builder, typeFinder, configuration);

        // Extract the LightInject container if possible
        if (builder is LightInjectContainerBuilder lightInjectBuilder)
        {
            _lightInjectRegistrar.Register(lightInjectBuilder.NativeContainer, typeFinder, configuration);
        }
    }

    /// <summary>
    /// Creates an instance of the LightInjectDependencyRegistrarAdapter from a Type representing an ILightInjectDependencyRegistrar
    /// </summary>
    /// <param name="lightInjectRegistrarType">Type that implements ILightInjectDependencyRegistrar</param>
    /// <returns>A new instance of LightInjectDependencyRegistrarAdapter</returns>
    /// <exception cref="ArgumentException">Thrown when the provided type doesn't implement ILightInjectDependencyRegistrar</exception>
    /// <exception cref="MissingMethodException">Thrown when the ILightInjectDependencyRegistrar type doesn't have a parameterless constructor</exception>
    public static LightInjectDependencyRegistrarAdapter CreateFromType(Type lightInjectRegistrarType)
    {
        if (!typeof(ILightInjectDependencyRegistrar).IsAssignableFrom(lightInjectRegistrarType))
        {
            throw new ArgumentException($"Type {lightInjectRegistrarType.Name} must implement ILightInjectDependencyRegistrar", nameof(lightInjectRegistrarType));
        }

        // Create an instance of the ILightInjectDependencyRegistrar
        var lightInjectRegistrar = (ILightInjectDependencyRegistrar)Activator.CreateInstance(lightInjectRegistrarType)!;

        // Create and return the adapter
        return new LightInjectDependencyRegistrarAdapter(lightInjectRegistrar);
    }
}