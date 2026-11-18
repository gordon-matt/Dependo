namespace Dependo.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class DependoCatalogCollection
{
    public const string Name = "DependoCatalog";
}

[Collection(DependoCatalogCollection.Name)]
public sealed class DependoCatalogTests
{
    public DependoCatalogTests()
    {
        DependoCatalog.Reset();
    }

    [Fact]
    public void TryGetExports_BeforeSeal_ReturnsFalse()
    {
        DependoCatalog.RegisterService(typeof(ICatalogProbe));
        DependoCatalog.AddExport(typeof(ICatalogProbe), typeof(IncludedProbe));

        bool found = DependoCatalog.TryGetExports(
            typeof(ICatalogProbe),
            onlyConcreteClasses: true,
            includeAssembly: _ => true,
            out _);

        Assert.False(found);
        Assert.False(DependoCatalog.IsSealed);
    }

    [Fact]
    public void TryGetExports_AfterSeal_ReturnsExportedTypeOnce()
    {
        DependoCatalog.AddExport(typeof(ICatalogProbe), typeof(IncludedProbe));
        DependoCatalog.AddExport(typeof(ICatalogProbe), typeof(IncludedProbe));
        DependoCatalog.AddKnownAssembly(typeof(IncludedProbe).Assembly);
        DependoCatalog.Seal();

        var finder = new AppDomainTypeFinder
        {
            AssemblySkipLoadingPattern = ".*"
        };

        var found = finder.FindClassesOfType<ICatalogProbe>().ToList();

        Assert.Equal([typeof(IncludedProbe)], found);
    }

    [Fact]
    public void TryGetExports_AfterSeal_IncludesPluginAssembly()
    {
        DependoCatalog.RegisterService(typeof(ICatalogProbe));
        DependoCatalog.Seal();

        bool found = DependoCatalog.TryGetExports(
            typeof(ICatalogProbe),
            onlyConcreteClasses: true,
            includeAssembly: assembly => assembly == typeof(IncludedProbe).Assembly,
            out var types);

        Assert.True(found);
        Assert.Contains(typeof(IncludedProbe), types);
        Assert.Contains(typeof(ExcludedProbe), types);
    }

    [Fact]
    public void RegisterService_AfterSeal_DoesNotHideHostTypes()
    {
        DependoCatalog.Seal();
        DependoCatalog.RegisterService(typeof(ICatalogProbe));
        DependoCatalog.AddExport(typeof(ICatalogProbe), typeof(IncludedProbe));

        bool found = DependoCatalog.TryGetExports(
            typeof(ICatalogProbe),
            onlyConcreteClasses: true,
            includeAssembly: _ => false,
            out _);

        Assert.False(found);
    }
}

[Collection(DependoCatalogCollection.Name)]
public sealed class AppDomainTypeFinderFilterTests
{
    public AppDomainTypeFinderFilterTests()
    {
        DependoCatalog.Reset();
    }

    [Fact]
    public void FindClassesOfType_SkipPattern_DoesNotReflectMatchingAssemblies()
    {
        var finder = new AppDomainTypeFinder
        {
            AssemblySkipLoadingPattern = ".*"
        };

        var found = finder.FindClassesOfType<ICatalogProbe>();

        Assert.Empty(found);
    }
}

/// <summary>A service used only to prove the catalog does not scan past its exports.</summary>
public interface ICatalogProbe;

/// <summary>Included in the catalog by the tests.</summary>
public sealed class IncludedProbe : ICatalogProbe;

/// <summary>Present in the test assembly, and returned only when that assembly is scanned.</summary>
public sealed class ExcludedProbe : ICatalogProbe;