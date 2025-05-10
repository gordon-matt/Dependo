using System.Runtime.CompilerServices;

namespace Dependo;

/// <summary>
/// Provides access to the singleton engine instance that manages the application's dependencies
/// </summary>
public static class EngineContext
{
    private static IEngine? _engine;

    /// <summary>
    /// Gets the singleton engine instance
    /// </summary>
    public static IEngine Current =>
        _engine ?? throw new InvalidOperationException("Engine is not initialized. Call Initialize or Create first.");

    /// <summary>
    /// Creates and initializes a new engine instance
    /// </summary>
    /// <param name="engine">The engine implementation to use</param>
    /// <returns>The engine instance</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static IEngine Create(IEngine engine) => _engine = engine;

    /// <summary>
    /// Initializes a new engine instance based on the container type
    /// </summary>
    /// <param name="containerType">The container type name (Autofac, Lamar, Dryloc, etc.)</param>
    /// <returns>The engine instance</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static IEngine Initialize(string containerType) =>
        _engine ??= EngineFactory.CreateEngine(containerType);

    /// <summary>
    /// Replaces the engine instance with a new one
    /// </summary>
    /// <param name="engine">The new engine instance</param>
    /// <returns>The new engine instance</returns>
    public static IEngine Replace(IEngine engine)
    {
        // Dispose the old engine if possible
        if (_engine is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return _engine = engine;
    }
}