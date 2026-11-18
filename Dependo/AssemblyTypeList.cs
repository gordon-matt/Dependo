using System.Reflection;

namespace Dependo;

/// <summary>
/// Caches <see cref="Assembly.GetTypes"/> and matches assignable types.
/// </summary>
internal static class AssemblyTypeList
{
    private static readonly Dictionary<Assembly, Type[]> cache = [];

    /// <summary>
    /// Returns the types in <paramref name="assembly"/>, including the types that survived a
    /// <see cref="ReflectionTypeLoadException"/>. The result is cached for the process.
    /// </summary>
    public static Type[] GetTypes(Assembly assembly)
    {
        lock (cache)
        {
            if (cache.TryGetValue(assembly, out var cached))
            {
                return cached;
            }
        }

        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(type => type != null).Select(type => type!).ToArray();
        }

        lock (cache)
        {
            cache[assembly] = types;
        }

        return types;
    }

    /// <summary>
    /// Returns true when <paramref name="type"/> is a concrete class assignable to <paramref name="assignTypeFrom"/>.
    /// </summary>
    public static bool IsConcreteMatch(Type type, Type assignTypeFrom) =>
        !type.IsInterface &&
        !type.IsAbstract &&
        (
            assignTypeFrom.IsAssignableFrom(type) ||
            (
                assignTypeFrom.IsGenericTypeDefinition &&
                ImplementsOpenGeneric(type, assignTypeFrom)
            )
        );

    /// <summary>
    /// Returns true when <paramref name="type"/> implements <paramref name="openGeneric"/>.
    /// Every implemented generic interface is checked. The first one is not necessarily the match.
    /// </summary>
    internal static bool ImplementsOpenGeneric(Type type, Type openGeneric)
    {
        try
        {
            var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
            foreach (var implementedInterface in type.GetInterfaces())
            {
                if (!implementedInterface.IsGenericType)
                {
                    continue;
                }

                if (genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition()))
                {
                    return true;
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}