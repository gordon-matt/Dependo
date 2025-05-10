using System.Reflection;

namespace Dependo;

/// <summary>
/// Interface for classes that can find types in assemblies
/// </summary>
public interface ITypeFinder
{
    /// <summary>
    /// Find classes of the specified type
    /// </summary>
    /// <typeparam name="T">Base type to find derived types of</typeparam>
    /// <param name="onlyConcreteClasses">A value indicating whether to find only concrete classes</param>
    /// <returns>Collection of found types</returns>
    IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true);

    /// <summary>
    /// Find classes of the specified type
    /// </summary>
    /// <param name="assignTypeFrom">Base type to find derived types of</param>
    /// <param name="onlyConcreteClasses">A value indicating whether to find only concrete classes</param>
    /// <returns>Collection of found types</returns>
    IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true);

    /// <summary>
    /// Find classes of the specified type in the specified assemblies
    /// </summary>
    /// <typeparam name="T">Base type to find derived types of</typeparam>
    /// <param name="assemblies">Assemblies to search in</param>
    /// <param name="onlyConcreteClasses">A value indicating whether to find only concrete classes</param>
    /// <returns>Collection of found types</returns>
    IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true);

    /// <summary>
    /// Find classes of the specified type in the specified assemblies
    /// </summary>
    /// <param name="assignTypeFrom">Base type to find derived types of</param>
    /// <param name="assemblies">Assemblies to search in</param>
    /// <param name="onlyConcreteClasses">A value indicating whether to find only concrete classes</param>
    /// <returns>Collection of found types</returns>
    IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true);

    /// <summary>
    /// Gets the assemblies related to the current implementation
    /// </summary>
    /// <returns>A list of assemblies</returns>
    IList<Assembly> GetAssemblies();
}