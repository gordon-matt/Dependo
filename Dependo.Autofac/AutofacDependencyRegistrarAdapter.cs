using Microsoft.Extensions.Configuration;

namespace Dependo.Autofac;

/// <summary>
/// Adapter class to bridge the Autofac-specific IDependencyRegistrar with the framework-agnostic interface.
/// </summary>
public class AutofacDependencyRegistrarAdapter : IDependencyRegistrarAdapter
{
    private readonly IAutofacDependencyRegistrar _autofacRegistrar;

    /// <summary>
    /// Initializes a new instance of the AutofacDependencyRegistrarAdapter class
    /// </summary>
    /// <param name="autofacRegistrar">Autofac-specific registrar</param>
    public AutofacDependencyRegistrarAdapter(IAutofacDependencyRegistrar autofacRegistrar)
    {
        _autofacRegistrar = autofacRegistrar;
    }

    /// <inheritdoc/>
    public int Order => _autofacRegistrar.Order;

    /// <inheritdoc/>
    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
        // First handle any registrations made via the generic IDependencyRegistrar interface
        _autofacRegistrar.Register(builder, typeFinder, configuration);

        // Extract the Autofac container builder if possible
        if (builder is AutofacContainerBuilder autofacBuilder)
        {
            _autofacRegistrar.Register(autofacBuilder.NativeBuilder, typeFinder, configuration);
        }
    }

    /// <summary>
    /// Creates an instance of the AutofacDependencyRegistrarAdapter from a Type representing an IAutofacDependencyRegistrar
    /// </summary>
    /// <param name="autofacRegistrarType">Type that implements IAutofacDependencyRegistrar</param>
    /// <returns>A new instance of AutofacDependencyRegistrarAdapter</returns>
    /// <exception cref="ArgumentException">Thrown when the provided type doesn't implement IAutofacDependencyRegistrar</exception>
    /// <exception cref="MissingMethodException">Thrown when the IAutofacDependencyRegistrar type doesn't have a parameterless constructor</exception>
    public static AutofacDependencyRegistrarAdapter CreateFromType(Type autofacRegistrarType)
    {
        if (!typeof(IAutofacDependencyRegistrar).IsAssignableFrom(autofacRegistrarType))
        {
            throw new ArgumentException($"Type {autofacRegistrarType.Name} must implement IAutofacDependencyRegistrar", nameof(autofacRegistrarType));
        }

        // Create an instance of the IAutofacDependencyRegistrar
        var autofacRegistrar = (IAutofacDependencyRegistrar)Activator.CreateInstance(autofacRegistrarType)!;

        // Create and return the adapter
        return new AutofacDependencyRegistrarAdapter(autofacRegistrar);
    }
}