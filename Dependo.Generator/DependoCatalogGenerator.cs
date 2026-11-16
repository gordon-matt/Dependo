using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Dependo.Generator;

/// <summary>
/// Writes a module initializer that publishes <c>IDependencyRegistrar</c> implementations and
/// <c>[DependoExports]</c> services into <c>DependoCatalog</c>.
/// </summary>
[Generator]
public sealed class DependoCatalogGenerator : IIncrementalGenerator
{
    private const string RegistrarMetadataName = "Dependo.IDependencyRegistrar";
    private const string ExportsMetadataName = "Dependo.DependoExportsAttribute";
    private const string CatalogMetadataName = "Dependo.DependoCatalogAssemblyAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var mode = context.AnalyzerConfigOptionsProvider.Select(static (options, _) =>
        {
            options.GlobalOptions.TryGetValue("build_property.Dependocatalog", out var value);
            return value ?? string.Empty;
        });

        context.RegisterSourceOutput(context.CompilationProvider.Combine(mode), static (source, input) =>
        {
            Execute(input.Left, input.Right, source);
        });
    }

    private static void Execute(Compilation compilation, string mode, SourceProductionContext source)
    {
        if (string.Equals(mode, "off", System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var registrar = compilation.GetTypeByMetadataName(RegistrarMetadataName);
        if (registrar == null)
        {
            return;
        }

        bool forceRoot = string.Equals(mode, "root", System.StringComparison.OrdinalIgnoreCase);
        bool forceLocal = string.Equals(mode, "local", System.StringComparison.OrdinalIgnoreCase);
        bool isRoot = forceRoot || (!forceLocal && compilation.GetEntryPoint(System.Threading.CancellationToken.None) != null);

        var exportServices = GetExportServices(compilation).ToList();
        var registrarServices = GetRegistrarInterfaces(compilation, registrar).ToList();
        var exports = FindExports(compilation, registrar, exportServices);

        if (!isRoot && exports.Count == 0)
        {
            return;
        }

        string sourceText = isRoot
            ? WriteRoot(compilation, exports, exportServices, registrarServices)
            : WriteLocal(exports);

        source.AddSource("DependoCatalog.g.cs", SourceText.From(sourceText, Encoding.UTF8));
    }

    private static List<ServiceExport> FindExports(
        Compilation compilation,
        INamedTypeSymbol registrar,
        List<INamedTypeSymbol> exportServices)
    {
        var exports = new List<ServiceExport>();
        foreach (var type in GetNamespaceTypes(compilation.Assembly.GlobalNamespace))
        {
            if (!IsConcreteClass(type) || !IsAccessible(type))
            {
                continue;
            }

            if (Implements(type, registrar))
            {
                foreach (var implemented in type.AllInterfaces)
                {
                    if (SymbolEqualityComparer.Default.Equals(implemented, registrar)
                        || implemented.AllInterfaces.Any(candidate => SymbolEqualityComparer.Default.Equals(candidate, registrar)))
                    {
                        exports.Add(new ServiceExport(implemented, type));
                    }
                }
            }

            foreach (var service in exportServices)
            {
                if (Implements(type, service))
                {
                    exports.Add(new ServiceExport(service, type));
                }
            }
        }

        return exports
            .Distinct()
            .OrderBy(export => export.Service.ToDisplayString(), System.StringComparer.Ordinal)
            .ThenBy(export => export.Implementation.ToDisplayString(), System.StringComparer.Ordinal)
            .ToList();
    }

    private static IEnumerable<INamedTypeSymbol> GetExportServices(Compilation compilation)
    {
        var attribute = compilation.GetTypeByMetadataName(ExportsMetadataName);
        if (attribute == null)
        {
            yield break;
        }

        var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var assemblies = compilation.SourceModule.ReferencedAssemblySymbols.Concat(new[] { compilation.Assembly });
        foreach (var assembly in assemblies)
        {
            foreach (var data in assembly.GetAttributes())
            {
                if (!SymbolEqualityComparer.Default.Equals(data.AttributeClass, attribute))
                {
                    continue;
                }

                if (data.ConstructorArguments.Length == 0)
                {
                    continue;
                }

                foreach (var service in ReadTypes(data.ConstructorArguments[0]))
                {
                    if (seen.Add(service))
                    {
                        yield return service;
                    }
                }
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> ReadTypes(TypedConstant constant)
    {
        if (constant.Kind == TypedConstantKind.Array)
        {
            foreach (var value in constant.Values)
            {
                if (value.Value is INamedTypeSymbol type)
                {
                    yield return type;
                }
            }

            yield break;
        }

        if (constant.Value is INamedTypeSymbol single)
        {
            yield return single;
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetRegistrarInterfaces(Compilation compilation, INamedTypeSymbol registrar)
    {
        var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var assemblies = compilation.SourceModule.ReferencedAssemblySymbols
            .Concat(new[] { compilation.Assembly })
            .Where(assembly => (assembly.Name ?? string.Empty).StartsWith("Dependo", System.StringComparison.Ordinal));

        foreach (var assembly in assemblies)
        {
            foreach (var type in GetNamespaceTypes(assembly.GlobalNamespace))
            {
                if (type.TypeKind != TypeKind.Interface)
                {
                    continue;
                }

                if (SymbolEqualityComparer.Default.Equals(type, registrar)
                    || type.AllInterfaces.Any(candidate => SymbolEqualityComparer.Default.Equals(candidate, registrar)))
                {
                    if (seen.Add(type))
                    {
                        yield return type;
                    }
                }
            }
        }
    }

    private static string WriteLocal(List<ServiceExport> exports)
    {
        var builder = new StringBuilder();
        AppendHeader(builder);
        builder.AppendLine("[assembly: global::Dependo.DependoCatalogAssemblyAttribute]");
        builder.AppendLine();
        builder.AppendLine("namespace DependoGenerated");
        builder.AppendLine("{");
        builder.AppendLine("    internal static class Catalog");
        builder.AppendLine("    {");
        builder.AppendLine("        [global::System.Runtime.CompilerServices.ModuleInitializer]");
        builder.AppendLine("        internal static void Register()");
        builder.AppendLine("        {");
        builder.AppendLine("            global::Dependo.DependoCatalog.AddKnownAssembly(typeof(Catalog).Assembly);");
        AppendExports(builder, exports);
        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static string WriteRoot(
        Compilation compilation,
        List<ServiceExport> exports,
        List<INamedTypeSymbol> exportServices,
        List<INamedTypeSymbol> registrarServices)
    {
        var services = registrarServices
            .Concat(exportServices)
            .Distinct(SymbolNameComparer.Instance)
            .OrderBy(type => type.ToDisplayString(), System.StringComparer.Ordinal)
            .ToList();

        var builder = new StringBuilder();
        AppendHeader(builder);
        builder.AppendLine("[assembly: global::Dependo.DependoCatalogAssemblyAttribute]");
        builder.AppendLine();
        builder.AppendLine("namespace DependoGenerated");
        builder.AppendLine("{");
        builder.AppendLine("    internal static class Catalog");
        builder.AppendLine("    {");
        builder.AppendLine("        [global::System.Runtime.CompilerServices.ModuleInitializer]");
        builder.AppendLine("        internal static void Register()");
        builder.AppendLine("        {");
        builder.AppendLine("            global::Dependo.DependoCatalog.AddKnownAssembly(typeof(Catalog).Assembly);");

        foreach (var service in services)
        {
            builder.Append("            global::Dependo.DependoCatalog.RegisterService(");
            builder.Append(TypeOf(service));
            builder.AppendLine(");");
        }

        AppendExports(builder, exports);

        foreach (var assemblyName in GetParticipatingAssemblies(compilation))
        {
            builder.Append("            global::System.Reflection.Assembly.Load(\"");
            builder.Append(Escape(assemblyName));
            builder.AppendLine("\");");
        }

        builder.AppendLine("            global::Dependo.DependoCatalog.Seal();");
        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static void AppendExports(StringBuilder builder, List<ServiceExport> exports)
    {
        foreach (var export in exports)
        {
            builder.Append("            global::Dependo.DependoCatalog.AddExport(");
            builder.Append(TypeOf(export.Service));
            builder.Append(", ");
            builder.Append(TypeOf(export.Implementation));
            builder.AppendLine(");");
        }
    }

    private static IEnumerable<string> GetParticipatingAssemblies(Compilation compilation)
    {
        var attribute = compilation.GetTypeByMetadataName(CatalogMetadataName);
        if (attribute == null)
        {
            yield break;
        }

        var names = new List<string>();
        foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            bool participates = assembly.GetAttributes().Any(data =>
                SymbolEqualityComparer.Default.Equals(data.AttributeClass, attribute));
            if (participates && !string.IsNullOrEmpty(assembly.Name))
            {
                names.Add(assembly.Name!);
            }
        }

        names.Sort(System.StringComparer.Ordinal);
        foreach (var name in names)
        {
            yield return name;
        }
    }

    private static void AppendHeader(StringBuilder builder)
    {
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
    }

    private static string TypeOf(ITypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol named && named.IsGenericType)
        {
            var unbound = named.ConstructUnboundGenericType();
            return "typeof(" + unbound.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ")";
        }

        return "typeof(" + symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) + ")";
    }

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private static bool IsConcreteClass(INamedTypeSymbol type)
    {
        if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.IsStatic)
        {
            return false;
        }

        if (type.Name.Contains("<") || type.IsImplicitlyDeclared)
        {
            return false;
        }

        return type.ContainingNamespace?.ToDisplayString().StartsWith("DependoGenerated", System.StringComparison.Ordinal) != true;
    }

    private static bool IsAccessible(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type; current != null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
            {
                return false;
            }
        }

        return true;
    }

    private static bool Implements(INamedTypeSymbol type, INamedTypeSymbol service)
    {
        if (SymbolEqualityComparer.Default.Equals(type, service))
        {
            return false;
        }

        bool genericDefinition = service.IsGenericType
            && SymbolEqualityComparer.Default.Equals(service, service.OriginalDefinition);

        if (service.TypeKind == TypeKind.Interface)
        {
            return type.AllInterfaces.Any(implemented => genericDefinition
                ? SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, service.OriginalDefinition)
                : SymbolEqualityComparer.Default.Equals(implemented, service));
        }

        for (var current = type.BaseType; current != null; current = current.BaseType)
        {
            bool matches = genericDefinition
                ? SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, service.OriginalDefinition)
                : SymbolEqualityComparer.Default.Equals(current, service);
            if (matches)
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<INamedTypeSymbol> GetNamespaceTypes(INamespaceSymbol ns)
    {
        foreach (var member in ns.GetMembers())
        {
            if (member is INamespaceSymbol nested)
            {
                foreach (var type in GetNamespaceTypes(nested))
                {
                    yield return type;
                }
            }
            else if (member is INamedTypeSymbol type)
            {
                yield return type;
                foreach (var nestedType in GetNestedTypes(type))
                {
                    yield return nestedType;
                }
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> GetNestedTypes(INamedTypeSymbol type)
    {
        foreach (var nested in type.GetTypeMembers())
        {
            yield return nested;
            foreach (var deeper in GetNestedTypes(nested))
            {
                yield return deeper;
            }
        }
    }

    private readonly struct ServiceExport : System.IEquatable<ServiceExport>
    {
        public ServiceExport(INamedTypeSymbol service, INamedTypeSymbol implementation)
        {
            Service = service;
            Implementation = implementation;
        }

        public INamedTypeSymbol Service { get; }

        public INamedTypeSymbol Implementation { get; }

        public bool Equals(ServiceExport other) =>
            SymbolEqualityComparer.Default.Equals(Service, other.Service)
            && SymbolEqualityComparer.Default.Equals(Implementation, other.Implementation);

        public override bool Equals(object? obj) => obj is ServiceExport other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = SymbolEqualityComparer.Default.GetHashCode(Service);
                return (hash * 397) ^ SymbolEqualityComparer.Default.GetHashCode(Implementation);
            }
        }
    }

    private sealed class SymbolNameComparer : System.Collections.Generic.IEqualityComparer<INamedTypeSymbol>
    {
        public static readonly SymbolNameComparer Instance = new();

        public bool Equals(INamedTypeSymbol? x, INamedTypeSymbol? y) =>
            SymbolEqualityComparer.Default.Equals(x, y);

        public int GetHashCode(INamedTypeSymbol obj) =>
            SymbolEqualityComparer.Default.GetHashCode(obj);
    }
}
