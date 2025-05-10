namespace Dependo;

/// <summary>
/// Factory for creating and configuring application dependo containers
/// </summary>
public static class DependoContainerFactory
{
    /// <summary>
    /// Creates an IDependoContainer implementation based on the container type
    /// </summary>
    /// <param name="containerType">The container type name (Autofac, Lamar, Dryloc, etc.)</param>
    /// <returns>The dependo container implementation</returns>
    /// <exception cref="ArgumentException">Thrown if the container type is not supported</exception>
    public static IDependoContainer CreateDependoContainer(string containerType) => containerType.ToLowerInvariant() switch
    {
        "autofac" => CreateAutofacDependoContainer(),
        "dryioc" => CreateDryIocDependoContainer(),
        "lamar" => CreateLamarDependoContainer(),
        _ => throw new ArgumentException($"Unsupported container type: {containerType}", nameof(containerType))
    };

    private static IDependoContainer CreateAutofacDependoContainer()
    {
        // Create Autofac dependo container dynamically to avoid reference coupling
        var dependoContainerType = Type.GetType("Dependo.Autofac.AutofacDependoContainer, Dependo.Autofac");
        return dependoContainerType == null
            ? throw new InvalidOperationException("Dependo.Autofac assembly is not available")
            : (IDependoContainer)Activator.CreateInstance(dependoContainerType)!;
    }

    private static IDependoContainer CreateDryIocDependoContainer()
    {
        // Create DryIoc dependo container dynamically to avoid reference coupling
        var dependoContainerType = Type.GetType("Dependo.DryIoc.DryIocDependoContainer, Dependo.DryIoc");
        return dependoContainerType == null
            ? throw new InvalidOperationException("Dependo.DryIoc assembly is not available")
            : (IDependoContainer)Activator.CreateInstance(dependoContainerType)!;
    }

    private static IDependoContainer CreateLamarDependoContainer()
    {
        // Create Lamar dependo container dynamically to avoid reference coupling
        var dependoContainerType = Type.GetType("Dependo.Lamar.LamarDependoContainer, Dependo.Lamar");
        return dependoContainerType == null
            ? throw new InvalidOperationException("Dependo.Lamar assembly is not available")
            : (IDependoContainer)Activator.CreateInstance(dependoContainerType)!;
    }
}