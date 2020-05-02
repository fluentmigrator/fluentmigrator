using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FluentMigrator.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MigrationAttributeVersionShouldBeUnique : FluentMigratorAnalyzer
    {
        public const string DiagnosticId = "MigrationAttributeVersionShouldBeUnique";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.MigrationAttributeVersionShouldBeUniqueTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.MigrationAttributeVersionShouldBeUniqueMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.MigrationAttributeVersionShouldBeUniqueDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "FluentMigrator";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, FluentMigratorContext fluentMigratorContext)
        {
            compilationStartContext.RegisterSymbolAction(symbolContext =>
            {
                var typeSymbol = (ITypeSymbol)symbolContext.Symbol;
                var migrationVersion = GetMigrationAttributeVersion(fluentMigratorContext, typeSymbol.GetAttributes());
                if (migrationVersion == null)
                {
                    return;
                }

                foreach (var otherTypeName in symbolContext.Compilation.Assembly.TypeNames.Where(typeName => typeName != typeSymbol.MetadataName))
                {
                    var otherTypeSymbol = symbolContext.Compilation.GetTypeByMetadataName(otherTypeName);
                    var otherMigrationVersion = GetMigrationAttributeVersion(fluentMigratorContext, otherTypeSymbol.GetAttributes());
                    if (otherMigrationVersion == migrationVersion)
                    {
                        symbolContext.ReportDiagnostic(
                            Diagnostic.Create(
                                Rule,
                                typeSymbol.Locations.First(),
                                $"{typeSymbol.Name} and {otherTypeSymbol.Name}",
                                migrationVersion));
                        return;
                    }
                }
            }, SymbolKind.Method);
        }

        private static long? GetMigrationAttributeVersion(FluentMigratorContext fluentMigratorContext, IEnumerable<AttributeData> attributes)
            => attributes
                .FirstOrDefault(a => fluentMigratorContext.MigrationAttributeType.IsAssignableFrom(a.AttributeClass))
                ?.ConstructorArguments[0].Value as long?;
    }
}
