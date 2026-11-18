namespace Dependo;

/// <summary>
/// Tells the catalog generator which services this assembly, and assemblies that reference it, export.
/// </summary>
/// <remarks>
/// Place this on the assembly that defines the service, or on each assembly that contains implementations.
/// The generator records concrete implementations at compile time so startup does not have to call
/// <see cref="System.Reflection.Assembly.GetTypes"/> on every loaded assembly.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DependoExportsAttribute : Attribute
{
    /// <summary>
    /// Marks <paramref name="serviceTypes"/> for compile-time export.
    /// </summary>
    /// <param name="serviceTypes">Interfaces or base classes whose concrete implementations should be catalogued.</param>
    public DependoExportsAttribute(params Type[] serviceTypes)
    {
        ServiceTypes = serviceTypes ?? [];
    }

    /// <summary>
    /// Gets the services whose implementations are written into the catalog.
    /// </summary>
    public Type[] ServiceTypes { get; }
}