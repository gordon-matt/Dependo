using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependo.DotNetDefault;

/// <summary>
/// Adapter class to bridge the .NET Default-specific IDependencyRegistrar with the framework-agnostic interface.
/// </summary>
public class DotNetDefaultDependencyRegistrarAdapter : IDependencyRegistrarAdapter
{
    private readonly IDotNetDefaultDependencyRegistrar _dotNetDefaultRegistrar;

    /// <summary>
    /// Initializes a new instance of the DotNetDefaultDependencyRegistrarAdapter class
    /// </summary>
    /// <param name="dotNetDefaultRegistrar">.NET Default-specific registrar</param>
    public DotNetDefaultDependencyRegistrarAdapter(IDotNetDefaultDependencyRegistrar dotNetDefaultRegistrar)
    {
        _dotNetDefaultRegistrar = dotNetDefaultRegistrar;
    }

    /// <inheritdoc/>
    public int Order => _dotNetDefaultRegistrar.Order;

    /// <inheritdoc/>
    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
        // Extract the .NET Default service collection if possible
        if (builder is DotNetDefaultContainerBuilder dotNetDefaultBuilder)
        {
            _dotNetDefaultRegistrar.Register(dotNetDefaultBuilder.Services, typeFinder);
        }
    }

    /// <summary>
    /// Creates an instance of the DotNetDefaultDependencyRegistrarAdapter from a Type representing an IDotNetDefaultDependencyRegistrar
    /// </summary>
    /// <param name="dotNetDefaultRegistrarType">Type that implements IDotNetDefaultDependencyRegistrar</param>
    /// <returns>A new instance of DotNetDefaultDependencyRegistrarAdapter</returns>
    /// <exception cref="ArgumentException">Thrown when the provided type doesn't implement IDotNetDefaultDependencyRegistrar</exception>
    /// <exception cref="MissingMethodException">Thrown when the IDotNetDefaultDependencyRegistrar type doesn't have a parameterless constructor</exception>
    public static DotNetDefaultDependencyRegistrarAdapter CreateFromType(Type dotNetDefaultRegistrarType)
    {
        if (!typeof(IDotNetDefaultDependencyRegistrar).IsAssignableFrom(dotNetDefaultRegistrarType))
        {
            throw new ArgumentException($"Type {dotNetDefaultRegistrarType.Name} must implement IDotNetDefaultDependencyRegistrar", nameof(dotNetDefaultRegistrarType));
        }

        // Create an instance of the IDotNetDefaultDependencyRegistrar
        var dotNetDefaultRegistrar = (IDotNetDefaultDependencyRegistrar)Activator.CreateInstance(dotNetDefaultRegistrarType)!;

        // Create and return the adapter
        return new DotNetDefaultDependencyRegistrarAdapter(dotNetDefaultRegistrar);
    }
} 