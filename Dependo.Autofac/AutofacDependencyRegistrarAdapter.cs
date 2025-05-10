namespace Dependo.Autofac;

/// <summary>
/// Adapter class to bridge the Autofac-specific IDependencyRegistrar with the framework-agnostic interface.
/// </summary>
public class AutofacDependencyRegistrarAdapter : IDependencyRegistrar
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
    public void Register(IContainerBuilder builder, ITypeFinder typeFinder)
    {
        // Extract the Autofac container builder if possible
        if (builder is AutofacContainerBuilder autofacBuilder)
        {
            _autofacRegistrar.Register(autofacBuilder.NativeBuilder, typeFinder);
        }
    }
}