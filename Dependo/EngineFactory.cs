namespace Dependo;

/// <summary>
/// Engine factory for creating and configuring application engines
/// </summary>
public static class EngineFactory
{
    /// <summary>
    /// Creates an IEngine implementation based on the container type
    /// </summary>
    /// <param name="containerType">The container type name (Autofac, Lamar, Dryloc, etc.)</param>
    /// <returns>The engine implementation</returns>
    /// <exception cref="ArgumentException">Thrown if the container type is not supported</exception>
    public static IEngine CreateEngine(string containerType) => containerType.ToLowerInvariant() switch
    {
        "autofac" => CreateAutofacEngine(),
        "dryioc" => CreateDryIocEngine(),
        "lamar" => CreateLamarEngine(),
        _ => throw new ArgumentException($"Unsupported container type: {containerType}", nameof(containerType))
    };

    private static IEngine CreateAutofacEngine()
    {
        // Create Autofac engine dynamically to avoid reference coupling
        var engineType = Type.GetType("Dependo.Autofac.AutofacEngine, Dependo.Autofac");
        return engineType == null
            ? throw new InvalidOperationException("Dependo.Autofac assembly is not available")
            : (IEngine)Activator.CreateInstance(engineType)!;
    }

    private static IEngine CreateDryIocEngine()
    {
        // Create DryIoc engine dynamically to avoid reference coupling
        var engineType = Type.GetType("Dependo.DryIoc.DryIocEngine, Dependo.DryIoc");
        return engineType == null
            ? throw new InvalidOperationException("Dependo.DryIoc assembly is not available")
            : (IEngine)Activator.CreateInstance(engineType)!;
    }

    private static IEngine CreateLamarEngine()
    {
        // Create Lamar engine dynamically to avoid reference coupling
        var engineType = Type.GetType("Dependo.Lamar.LamarEngine, Dependo.Lamar");
        return engineType == null
            ? throw new InvalidOperationException("Dependo.Lamar assembly is not available")
            : (IEngine)Activator.CreateInstance(engineType)!;
    }
}