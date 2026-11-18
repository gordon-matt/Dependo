using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

    [Fact]
    public void IsAssemblySafeToLoad_AdditionalPrefix_SkipsMatchingAssembly()
    {
        DependoCatalog.SkipAssembliesStartingWith("AWSSDK", "Azure");

        var finder = new AppDomainTypeFinder();

        Assert.False(finder.IsAssemblySafeToLoad("AWSSDK.S3, Version=3.0.0.0, Culture=neutral, PublicKeyToken=null"));
        Assert.False(finder.IsAssemblySafeToLoad("azure.core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"));
        Assert.True(finder.IsAssemblySafeToLoad("Dependo.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"));
        Assert.False(finder.IsAssemblySafeToLoad("System.Runtime, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"));
    }

    [Fact]
    public void FindClassesOfType_AdditionalPrefix_DoesNotReflectMatchingAssemblies()
    {
        DependoCatalog.SkipAssembliesStartingWith(typeof(ExcludedProbe).Assembly.GetName().Name!.ToLowerInvariant());

        var found = new AppDomainTypeFinder().FindClassesOfType<ICatalogProbe>();

        Assert.Empty(found);
    }

    [Fact]
    public void TryGetExports_AdditionalPrefix_SkipsUncataloguedAssembly()
    {
        DependoCatalog.RegisterService(typeof(ICatalogProbe));
        DependoCatalog.Seal();
        DependoCatalog.SkipAssembliesStartingWith(typeof(ExcludedProbe).Assembly.GetName().Name!);

        var found = new AppDomainTypeFinder().FindClassesOfType<ICatalogProbe>();

        Assert.Empty(found);
    }

    [Fact]
    public void SkipAssembliesStartingWith_IgnoresDuplicatePrefixes()
    {
        DependoCatalog.SkipAssembliesStartingWith("Azure", " azure ", "AZURE");

        Assert.Equal(["Azure"], DependoCatalog.SkipAssemblyPrefixes);
    }

    [Fact]
    public void SkipAssembliesStartingWith_RejectsEmptyPrefix()
    {
        Assert.Throws<ArgumentException>(() => DependoCatalog.SkipAssembliesStartingWith("Azure", " "));
        Assert.Empty(DependoCatalog.SkipAssemblyPrefixes);
    }

    [Fact]
    public void Reset_ClearsSkipAssemblyPrefixes()
    {
        DependoCatalog.SkipAssembliesStartingWith("Azure");

        DependoCatalog.Reset();

        Assert.Empty(DependoCatalog.SkipAssemblyPrefixes);
    }

    [Fact]
    public void FindClassesOfType_OpenGeneric_MatchesEveryImplementedInterface()
    {
        var finder = new AppDomainTypeFinder();

        Assert.Contains(typeof(DualGenericProbe), finder.FindClassesOfType(typeof(IFirstGenericProbe<>)));
        Assert.Contains(typeof(DualGenericProbe), finder.FindClassesOfType(typeof(ISecondGenericProbe<>)));
    }

    [Fact]
    public void TryGetExports_OpenGeneric_MatchesEveryImplementedInterface()
    {
        DependoCatalog.RegisterService(typeof(IFirstGenericProbe<>));
        DependoCatalog.RegisterService(typeof(ISecondGenericProbe<>));
        DependoCatalog.Seal();

        var finder = new AppDomainTypeFinder();

        Assert.Contains(typeof(DualGenericProbe), finder.FindClassesOfType(typeof(IFirstGenericProbe<>)));
        Assert.Contains(typeof(DualGenericProbe), finder.FindClassesOfType(typeof(ISecondGenericProbe<>)));
    }

    [Fact]
    public void GetAssemblies_ReusesListUntilSettingsChange()
    {
        var finder = new AppDomainTypeFinder();

        IList<System.Reflection.Assembly> first = finder.GetAssemblies();
        IList<System.Reflection.Assembly> second = finder.GetAssemblies();

        Assert.Same(first, second);

        finder.LoadAppDomainAssemblies = false;
        IList<System.Reflection.Assembly> third = finder.GetAssemblies();

        Assert.NotSame(first, third);
        Assert.Empty(third);
    }

    [Fact]
    public void FindClassesOfType_AfterAbsorb_IncludesAssemblyLoadedLater()
    {
        DependoCatalog.RegisterService(typeof(ILateLoadedProbe));
        DependoCatalog.Seal();

        var finder = new AppDomainTypeFinder();
        Assert.Empty(finder.FindClassesOfType<ILateLoadedProbe>());

        System.Reflection.Assembly assembly = LoadLateProbeAssembly();
        Type probe = assembly.GetType("LateLoadedProbe")!;

        Assert.Contains(probe, finder.FindClassesOfType<ILateLoadedProbe>());
    }

    private static System.Reflection.Assembly LoadLateProbeAssembly()
    {
        var references = System.AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .ToList();

        string assemblyName = "DependoLateProbe_" + Guid.NewGuid().ToString("N");
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText("public sealed class LateLoadedProbe : global::Dependo.Tests.ILateLoadedProbe;")],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var stream = new MemoryStream();
        var emit = compilation.Emit(stream);
        Assert.True(emit.Success, string.Join(Environment.NewLine, emit.Diagnostics));
        stream.Position = 0;

        return new AssemblyLoadContext(assemblyName).LoadFromStream(stream);
    }
}

/// <summary>A service used only to prove the catalog does not scan past its exports.</summary>
public interface ICatalogProbe;

/// <summary>Included in the catalog by the tests.</summary>
public sealed class IncludedProbe : ICatalogProbe;

/// <summary>Present in the test assembly, and returned only when that assembly is scanned.</summary>
public sealed class ExcludedProbe : ICatalogProbe;

/// <summary>One of two generic interfaces implemented by <see cref="DualGenericProbe"/>.</summary>
public interface IFirstGenericProbe<T>;

/// <summary>The other generic interface implemented by <see cref="DualGenericProbe"/>.</summary>
public interface ISecondGenericProbe<T>;

/// <summary>Implements two unrelated generic interfaces, so a scan that stops at the first one misses a match.</summary>
public sealed class DualGenericProbe : IFirstGenericProbe<int>, ISecondGenericProbe<string>;

/// <summary>Implemented only by an assembly loaded after the catalog has already been queried.</summary>
public interface ILateLoadedProbe;