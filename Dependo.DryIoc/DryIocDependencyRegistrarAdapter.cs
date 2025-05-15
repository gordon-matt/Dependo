using Microsoft.Extensions.Configuration;

namespace Dependo.DryIoc;

/// <summary>
/// Adapter class to bridge the DryIoc-specific IDependencyRegistrar with the framework-agnostic interface.
/// </summary>
public class DryIocDependencyRegistrarAdapter : IDependencyRegistrarAdapter
{
    private readonly IDryIocDependencyRegistrar _dryIocRegistrar;

    /// <summary>
    /// Initializes a new instance of the DryIocDependencyRegistrarAdapter class
    /// </summary>
    /// <param name="dryIocRegistrar">DryIoc-specific registrar</param>
    public DryIocDependencyRegistrarAdapter(IDryIocDependencyRegistrar dryIocRegistrar)
    {
        _dryIocRegistrar = dryIocRegistrar;
    }

    /// <inheritdoc/>
    public int Order => _dryIocRegistrar.Order;

    /// <inheritdoc/>
    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
        // First handle any registrations made via the generic IDependencyRegistrar interface
        _dryIocRegistrar.Register(builder, typeFinder, configuration);

        // Extract the DryIoc container if possible
        if (builder is DryIocContainerBuilder dryIocBuilder)
        {
            _dryIocRegistrar.Register(dryIocBuilder.NativeContainer, typeFinder, configuration);
        }
    }

    /// <summary>
    /// Creates an instance of the DryIocDependencyRegistrarAdapter from a Type representing an IDryIocDependencyRegistrar
    /// </summary>
    /// <param name="dryIocRegistrarType">Type that implements IDryIocDependencyRegistrar</param>
    /// <returns>A new instance of DryIocDependencyRegistrarAdapter</returns>
    /// <exception cref="ArgumentException">Thrown when the provided type doesn't implement IDryIocDependencyRegistrar</exception>
    /// <exception cref="MissingMethodException">Thrown when the IDryIocDependencyRegistrar type doesn't have a parameterless constructor</exception>
    public static DryIocDependencyRegistrarAdapter CreateFromType(Type dryIocRegistrarType)
    {
        if (!typeof(IDryIocDependencyRegistrar).IsAssignableFrom(dryIocRegistrarType))
        {
            throw new ArgumentException($"Type {dryIocRegistrarType.Name} must implement IDryIocDependencyRegistrar", nameof(dryIocRegistrarType));
        }

        // Create an instance of the IDryIocDependencyRegistrar
        var dryIocRegistrar = (IDryIocDependencyRegistrar)Activator.CreateInstance(dryIocRegistrarType)!;

        // Create and return the adapter
        return new DryIocDependencyRegistrarAdapter(dryIocRegistrar);
    }
}