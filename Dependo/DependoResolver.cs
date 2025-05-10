using System.Runtime.CompilerServices;

namespace Dependo;

/// <summary>
/// Provides access to the singleton dependo container instance that manages the application's dependencies
/// </summary>
public static class DependoResolver
{
    private static IDependoContainer? instance;

    /// <summary>
    /// Gets the singleton dependo container instance
    /// </summary>
    public static IDependoContainer Instance =>
        instance ?? throw new InvalidOperationException("Dependo Container is not initialized. Call Initialize or Create first.");

    /// <summary>
    /// Creates and initializes a new dependo container instance
    /// </summary>
    /// <param name="dependoContainer">The dependo container implementation to use</param>
    /// <returns>The dependo container instance</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static IDependoContainer Create(IDependoContainer dependoContainer) => instance = dependoContainer;

    /// <summary>
    /// Initializes a new dependo container instance based on the container type
    /// </summary>
    /// <param name="containerType">The container type name (Autofac, Lamar, Dryloc, etc.)</param>
    /// <returns>The dependo container instance</returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static IDependoContainer Initialize(string containerType) =>
        instance ??= DependoContainerFactory.CreateDependoContainer(containerType);

    /// <summary>
    /// Replaces the dependo container instance with a new one
    /// </summary>
    /// <param name="dependoContainer">The new dependo container instance</param>
    /// <returns>The new engdependo containerine instance</returns>
    public static IDependoContainer Replace(IDependoContainer dependoContainer)
    {
        // Dispose the old dependo container if possible
        if (instance is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return instance = dependoContainer;
    }
}