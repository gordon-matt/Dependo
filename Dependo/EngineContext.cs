using System.Runtime.CompilerServices;

namespace Dependo;

/// <summary>
/// Provides access to the singleton engine instance that manages the application's dependencies
/// </summary>
public static class EngineContext
{
    /// <summary>
    /// Gets the singleton engine instance
    /// </summary>
    public static IEngine Current { get; private set; } = default!;

    /// <summary>
    /// Creates and initializes a new engine instance if not already created
    /// </summary>
    /// <param name="engine">The engine implementation to use</param>
    /// <returns>The engine instance</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static IEngine Create(IEngine engine) => Current ??= engine;
}