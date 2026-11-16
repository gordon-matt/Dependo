using System.Reflection;

namespace Dependo;

/// <summary>
/// Caches <see cref="Assembly.GetTypes"/> and matches assignable types.
/// </summary>
internal static class AssemblyTypeList
{
    private static readonly Dictionary<Assembly, Type[]> cache = new();

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
    public static bool IsConcreteMatch(Type type, Type assignTypeFrom)
    {
        if (type.IsInterface || type.IsAbstract)
        {
            return false;
        }

        if (assignTypeFrom.IsAssignableFrom(type))
        {
            return true;
        }

        return assignTypeFrom.IsGenericTypeDefinition && ImplementsOpenGeneric(type, assignTypeFrom);
    }

    /// <summary>
    /// Keeps the historical open-generic check used by assembly scanning.
    /// </summary>
    private static bool ImplementsOpenGeneric(Type type, Type openGeneric)
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

                return genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition());
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
