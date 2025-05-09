using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Dependo.Autofac;

/// <summary>
/// A class that finds types needed by Dependo by loading assemblies in the
/// currently executing AppDomain.
/// </summary>
internal class AppDomainTypeFinder : ITypeFinder
{
    private readonly bool ignoreReflectionErrors = true;

    /// <summary>The app domain to look for types in.</summary>
    public virtual AppDomain App => AppDomain.CurrentDomain;

    /// <summary>Gets or sets assemblies loaded at startup in addition to those loaded in the AppDomain.</summary>
    public IList<string> AssemblyNames { get; set; } = [];

    /// <summary>Gets or sets the pattern for assemblies that will be investigated.</summary>
    public string AssemblyRestrictToLoadingPattern { get; set; } = ".*";

    /// <summary>Gets the pattern for assemblies that we know don't need to be investigated.</summary>
    public string AssemblySkipLoadingPattern { get; set; } = "^System|^mscorlib|^Microsoft|^Autofac|^AutoMapper|^Castle|^EntityFramework|^EPPlus|^FluentValidation|^log4net|^Newtonsoft|^NLog|^Serilog|^SixLabors|^StackExchange|^Telerik";

    /// <summary>Gets or sets whether Dependo should iterate assemblies in the app domain when loading types.</summary>
    public bool LoadAppDomainAssemblies { get; set; } = true;

    /// <summary>
    /// Find classes of type
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="onlyConcreteClasses">A value indicating whether to find only concrete classes</param>
    /// <returns>Result</returns>
    public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true) =>
        FindClassesOfType(typeof(T), onlyConcreteClasses);

    /// <summary>
    /// Find classes of type
    /// </summary>
    /// <param name="assignTypeFrom">Assign type from</param>
    /// <param name="onlyConcreteClasses">A value indicating whether to find only concrete classes</param>
    /// <returns>Result</returns>
    /// <returns></returns>
    public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true) =>
        FindClassesOfType(assignTypeFrom, GetAssemblies(), onlyConcreteClasses);

    /// <summary>
    /// Find classes of type
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="assemblies">Assemblies</param>
    /// <param name="onlyConcreteClasses">A value indicating whether to find only concrete classes</param>
    /// <returns>Result</returns>
    public IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true) =>
        FindClassesOfType(typeof(T), assemblies, onlyConcreteClasses);

    /// <summary>
    /// Find classes of type
    /// </summary>
    /// <param name="assignTypeFrom">Assign type from</param>
    /// <param name="assemblies">Assemblies</param>
    /// <param name="onlyConcreteClasses">A value indicating whether to find only concrete classes</param>
    /// <returns>Result</returns>
    public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
    {
        var result = new List<Type>();
        try
        {
            foreach (var assembly in assemblies)
            {
                Type[]? types = null;
                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    //Entity Framework 6 doesn't allow getting types (throws an exception)
                    if (!ignoreReflectionErrors)
                    {
                        throw;
                    }
                }

                if (types == null)
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (assignTypeFrom.IsAssignableFrom(type) ||
                        (assignTypeFrom.IsGenericTypeDefinition && DoesTypeImplementOpenGeneric(type, assignTypeFrom)))
                    {
                        if (type.IsInterface)
                        {
                            continue;
                        }

                        if (onlyConcreteClasses)
                        {
                            if (type.IsClass && !type.IsAbstract)
                            {
                                result.Add(type);
                            }
                        }
                        else
                        {
                            result.Add(type);
                        }
                    }
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            var msg = string.Join(Environment.NewLine,
                ex.LoaderExceptions.Where(e => e != null).Select(e => e.Message));

            var fail = new Exception(msg, ex);
            Debug.WriteLine(fail.Message, fail);

            throw fail;
        }
        return result;
    }

    /// <summary>
    /// Gets the assemblies related to the current implementation.
    /// </summary>
    /// <returns>A list of assemblies</returns>
    public virtual IList<Assembly> GetAssemblies()
    {
        var addedAssemblyNames = new List<string>();
        var assemblies = new List<Assembly>();

        if (LoadAppDomainAssemblies)
        {
            AddAssembliesInAppDomain(addedAssemblyNames, assemblies);
        }

        AddConfiguredAssemblies(addedAssemblyNames, assemblies);

        return assemblies;
    }

    /// <summary>
    /// Check if a dll is one of the shipped dlls that we know don't need to be investigated.
    /// </summary>
    /// <param name="assemblyFullName">
    /// The name of the assembly to check.
    /// </param>
    /// <returns>
    /// True if the assembly should be loaded into Dependo.
    /// </returns>
    public virtual bool IsAssemblySafeToLoad(string assemblyFullName) =>
        !AssemblyNameMatches(assemblyFullName, AssemblySkipLoadingPattern)
            && AssemblyNameMatches(assemblyFullName, AssemblyRestrictToLoadingPattern);

    /// <summary>
    /// Adds specifically configured assemblies.
    /// </summary>
    /// <param name="addedAssemblyNames"></param>
    /// <param name="assemblies"></param>
    protected virtual void AddConfiguredAssemblies(List<string> addedAssemblyNames, List<Assembly> assemblies)
    {
        foreach (string assemblyName in AssemblyNames)
        {
            var assembly = Assembly.Load(assemblyName);
            if (!addedAssemblyNames.Contains(assembly.FullName!))
            {
                assemblies.Add(assembly);
                addedAssemblyNames.Add(assembly.FullName!);
            }
        }
    }

    /// <summary>
    /// Does assembly name match pattern?
    /// </summary>
    /// <param name="assemblyFullName">
    /// The assembly name to match.
    /// </param>
    /// <param name="pattern">
    /// The regular expression pattern to match against the assembly name.
    /// </param>
    /// <returns>
    /// True if the pattern matches the assembly name.
    /// </returns>
    protected virtual bool AssemblyNameMatches(string assemblyFullName, string pattern) =>
        Regex.IsMatch(assemblyFullName, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Does type implement generic?
    /// </summary>
    /// <param name="type"></param>
    /// <param name="openGeneric"></param>
    /// <returns></returns>
    protected virtual bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
    {
        try
        {
            var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
            foreach (var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
            {
                if (!implementedInterface.IsGenericType)
                {
                    continue;
                }

                var isMatch = genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition());
                return isMatch;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Makes sure matching assemblies in the supplied folder are loaded in the app domain.
    /// </summary>
    /// <param name="directoryPath">
    /// The physical path to a directory containing dlls to load in the app domain.
    /// </param>
    protected virtual void LoadMatchingAssemblies(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        var loadedAssemblyNames = new List<string>();
        foreach (var assembly in GetAssemblies())
        {
            loadedAssemblyNames.Add(assembly.FullName!);
        }

        foreach (string dllPath in Directory.GetFiles(directoryPath, "*.dll"))
        {
            try
            {
                var an = AssemblyName.GetAssemblyName(dllPath);
                if (IsAssemblySafeToLoad(an.FullName) && !loadedAssemblyNames.Contains(an.FullName))
                {
                    App.Load(an);
                }
            }
            catch (BadImageFormatException ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }
    }

    /// <summary>
    /// Iterates all assemblies in the AppDomain and if it's name matches the configured patterns add it to our list.
    /// </summary>
    /// <param name="addedAssemblyNames"></param>
    /// <param name="assemblies"></param>
    private void AddAssembliesInAppDomain(List<string> addedAssemblyNames, List<Assembly> assemblies)
    {
        foreach (var assembly in App.GetAssemblies())
        {
            if (!assembly.IsDynamic && !addedAssemblyNames.Contains(assembly.FullName!))
            {
                assemblies.Add(assembly);
                addedAssemblyNames.Add(assembly.FullName!);
            }
        }
    }
}