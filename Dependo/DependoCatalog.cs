using System.Reflection;

namespace Dependo;

/// <summary>
/// Compile-time list of dependency registrars and other exported services.
/// </summary>
/// <remarks>
/// <para>
/// Each participating assembly gets a module initializer that calls <see cref="AddExport"/>.
/// The application assembly then calls <see cref="Seal"/>. After that, <see cref="ITypeFinder"/>
/// reads this catalog instead of calling <see cref="Assembly.GetTypes"/> on every loaded assembly.
/// </para>
/// <para>
/// Assemblies loaded later, such as plugins, are still inspected once. Only those assemblies are
/// reflected, and framework assemblies that match the type finder's skip pattern are ignored.
/// Set <see cref="ProbeBinFolder"/> to <see langword="false"/> when plugins are loaded by the host
/// before the container is built, so startup does not also load every DLL in the output folder.
/// </para>
/// </remarks>
public static class DependoCatalog
{
    private static readonly object gate = new();
    private static readonly HashSet<Type> knownServices = [];
    private static readonly Dictionary<Type, List<Type>> exports = new();
    private static readonly HashSet<Assembly> knownAssemblies = [];
    private static readonly HashSet<Assembly> scannedAssemblies = [];
    private static bool isSealed;

    /// <summary>
    /// Gets or sets whether a sealed catalog may load every eligible DLL from the application directory
    /// before resolving exports. The default is <see langword="true"/> so hosts that discover plugins by
    /// probing the output folder keep doing so. Set this to <see langword="false"/> before the container
    /// is built when every plugin assembly is already loaded.
    /// </summary>
    public static bool ProbeBinFolder { get; set; } = true;

    /// <summary>
    /// Gets a value indicating whether the application assembly has finished publishing compile-time exports.
    /// </summary>
    public static bool IsSealed
    {
        get
        {
            lock (gate)
            {
                return isSealed;
            }
        }
    }

    /// <summary>
    /// Records <paramref name="serviceType"/> as a service the catalog can answer without a full assembly scan.
    /// </summary>
    /// <param name="serviceType">The interface or base class callers will ask the type finder for.</param>
    public static void RegisterService(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);

        lock (gate)
        {
            // A service introduced after Seal was not indexed in the compile-time assemblies.
            // Leave it unknown so the type finder keeps the full scan for that service.
            if (isSealed && !knownServices.Contains(serviceType))
            {
                return;
            }

            knownServices.Add(serviceType);
            if (!exports.ContainsKey(serviceType))
            {
                exports[serviceType] = [];
            }
        }
    }

    /// <summary>
    /// Adds one concrete implementation of <paramref name="serviceType"/>.
    /// </summary>
    /// <param name="serviceType">The service the implementation is exported as.</param>
    /// <param name="implementationType">The concrete class to return from the type finder.</param>
    public static void AddExport(Type serviceType, Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(implementationType);

        lock (gate)
        {
            if (isSealed && !knownServices.Contains(serviceType))
            {
                return;
            }

            knownServices.Add(serviceType);
            if (!exports.TryGetValue(serviceType, out var implementations))
            {
                implementations = [];
                exports[serviceType] = implementations;
            }

            if (!implementations.Contains(implementationType))
            {
                implementations.Add(implementationType);
            }
        }
    }

    /// <summary>
    /// Marks <paramref name="assembly"/> as already indexed so plugin top-up does not reflect over it.
    /// </summary>
    /// <param name="assembly">An assembly whose exports were written by its module initializer.</param>
    public static void AddKnownAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        lock (gate)
        {
            knownAssemblies.Add(assembly);
        }
    }

    /// <summary>
    /// Finishes compile-time publication. Later calls are ignored.
    /// </summary>
    public static void Seal()
    {
        lock (gate)
        {
            isSealed = true;
        }
    }

    /// <summary>
    /// Gets the catalogued implementations of <paramref name="serviceType"/> when the catalog is sealed
    /// and that service was registered. Also picks up concrete matches from assemblies loaded afterwards.
    /// </summary>
    /// <param name="serviceType">The interface or base class to resolve.</param>
    /// <param name="onlyConcreteClasses">When <see langword="true"/>, abstract classes are removed.</param>
    /// <param name="includeAssembly">
    /// Decides whether an assembly that was not part of the compile-time catalog may be reflected.
    /// The type finder passes its skip and restrict patterns here.
    /// </param>
    /// <param name="types">The implementations, when this method returns <see langword="true"/>.</param>
    /// <returns>
    /// <see langword="true"/> when the catalog answers the query; <see langword="false"/> when the caller
    /// should scan assemblies itself.
    /// </returns>
    public static bool TryGetExports(
        Type serviceType,
        bool onlyConcreteClasses,
        Func<Assembly, bool> includeAssembly,
        out IReadOnlyList<Type> types)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(includeAssembly);

        lock (gate)
        {
            if (!isSealed || !knownServices.Contains(serviceType))
            {
                types = [];
                return false;
            }

            AbsorbLoadedAssemblies(includeAssembly);

            if (!exports.TryGetValue(serviceType, out var implementations))
            {
                types = [];
                return true;
            }

            types = onlyConcreteClasses
                ? implementations.Where(type => type.IsClass && !type.IsAbstract).ToArray()
                : implementations.ToArray();
            return true;
        }
    }

    /// <summary>
    /// Clears the catalog. Tests use this so one fixture does not leak exports into another.
    /// </summary>
    internal static void Reset()
    {
        lock (gate)
        {
            isSealed = false;
            ProbeBinFolder = true;
            knownServices.Clear();
            exports.Clear();
            knownAssemblies.Clear();
            scannedAssemblies.Clear();
        }
    }

    private static void AbsorbLoadedAssemblies(Func<Assembly, bool> includeAssembly)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic || knownAssemblies.Contains(assembly) || !scannedAssemblies.Add(assembly))
            {
                continue;
            }

            if (string.IsNullOrEmpty(assembly.FullName) || !includeAssembly(assembly))
            {
                continue;
            }

            foreach (var type in AssemblyTypeList.GetTypes(assembly))
            {
                foreach (var serviceType in knownServices)
                {
                    if (AssemblyTypeList.IsConcreteMatch(type, serviceType))
                    {
                        if (!exports.TryGetValue(serviceType, out var implementations))
                        {
                            implementations = [];
                            exports[serviceType] = implementations;
                        }

                        if (!implementations.Contains(type))
                        {
                            implementations.Add(type);
                        }
                    }
                }
            }
        }
    }
}
