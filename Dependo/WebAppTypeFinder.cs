using System.Reflection;

namespace Dependo;

/// <summary>
/// Provides information about types in the current web application.
/// Specifically looks at assemblies in the bin folder.
/// </summary>
public class WebAppTypeFinder : AppDomainTypeFinder
{
    private bool binFolderAssembliesLoaded;

    /// <summary>
    /// Gets or sets whether assemblies in the bin folder of the web application should be specifically checked for being loaded on application load.
    /// This is needed in situations where plugins need to be loaded in the AppDomain after the application has been reloaded.
    /// </summary>
    public bool EnsureBinFolderAssembliesLoaded { get; set; } = true;

    /// <summary>
    /// Get assemblies
    /// </summary>
    /// <returns>Result</returns>
    public override IList<Assembly> GetAssemblies()
    {
        EnsureCatalogAssembliesLoaded();
        return base.GetAssemblies();
    }

    /// <inheritdoc />
    protected override void EnsureCatalogAssembliesLoaded()
    {
        // ProbeBinFolder stays on for hosts that discover plugins by loading every DLL.
        // A host that loads plugins itself turns it off and avoids that walk at startup.
        if (!DependoCatalog.ProbeBinFolder || !EnsureBinFolderAssembliesLoaded || binFolderAssembliesLoaded)
        {
            return;
        }

        binFolderAssembliesLoaded = true;
        LoadMatchingAssemblies(GetBinDirectory());
    }

    /// <summary>
    /// Gets the bin directory path
    /// </summary>
    /// <returns>The physical path to the bin directory</returns>
    public virtual string GetBinDirectory() => AppContext.BaseDirectory;
}