namespace Dependo;

/// <summary>
/// Marks an assembly whose module initializer fills <see cref="DependoCatalog"/>.
/// </summary>
/// <remarks>
/// The host generator loads every referenced assembly that carries this attribute before it seals the catalog.
/// Loading the assembly runs its initializer, which is what publishes that assembly's exports.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class DependoCatalogAssemblyAttribute : Attribute;
