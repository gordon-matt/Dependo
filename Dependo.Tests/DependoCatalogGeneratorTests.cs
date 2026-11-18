using Dependo.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Dependo.Tests;

public sealed class DependoCatalogGeneratorTests
{
    [Fact]
    public void Library_EmitsRegistrarExport()
    {
        string generated = Generate(OutputKind.DynamicallyLinkedLibrary, """
            using Dependo;
            using Microsoft.Extensions.Configuration;

            public class SampleRegistrar : IDependencyRegistrar
            {
                public int Order => 1;

                public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
                {
                }
            }
            """);

        Assert.Contains("SampleRegistrar", generated, System.StringComparison.Ordinal);
        Assert.Contains("AddExport", generated, System.StringComparison.Ordinal);
        Assert.Contains("DependoCatalogAssemblyAttribute", generated, System.StringComparison.Ordinal);
        Assert.DoesNotContain("Seal()", generated, System.StringComparison.Ordinal);
    }

    [Fact]
    public void Host_SealsAndLoadsParticipatingAssemblies()
    {
        string generated = Generate(OutputKind.ConsoleApplication, """
            using Dependo;
            using Microsoft.Extensions.Configuration;

            public static class Program
            {
                public static void Main()
                {
                }
            }

            public class SampleRegistrar : IDependencyRegistrar
            {
                public int Order => 1;

                public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
                {
                }
            }
            """);

        Assert.Contains("RegisterService(typeof(global::Dependo.IDependencyRegistrar))", generated, System.StringComparison.Ordinal);
        Assert.Contains("SampleRegistrar", generated, System.StringComparison.Ordinal);
        Assert.Contains("Seal()", generated, System.StringComparison.Ordinal);
    }

    [Fact]
    public void Library_EmitsMarkedExport()
    {
        string generated = Generate(OutputKind.DynamicallyLinkedLibrary, """
            using Dependo;

            [assembly: DependoExports(typeof(IWidget))]
            public interface IWidget;

            public sealed class ClockWidget : IWidget;
            """);

        Assert.Contains("IWidget", generated, System.StringComparison.Ordinal);
        Assert.Contains("ClockWidget", generated, System.StringComparison.Ordinal);
        Assert.DoesNotContain("abstract", generated, System.StringComparison.Ordinal);
    }

    private static string Generate(OutputKind outputKind, string source)
    {
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
            .Distinct()
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .ToList();

        var compilation = CSharpCompilation.Create(
            "DependoCatalogGeneratorTests",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(outputKind));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(new DependoCatalogGenerator());
        driver = driver.RunGenerators(compilation);
        var result = driver.GetRunResult();

        Assert.Empty(result.Diagnostics);
        Assert.Single(result.Results);
        Assert.Empty(result.Results[0].Diagnostics);
        var sources = result.Results[0].GeneratedSources;
        Assert.Single(sources);
        return sources[0].SourceText.ToString();
    }
}