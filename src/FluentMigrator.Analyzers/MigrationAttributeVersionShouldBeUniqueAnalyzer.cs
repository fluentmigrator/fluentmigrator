#region License
// Copyright (c) 2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using FluentMigrator.Analyzers.Analysis;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FluentMigrator.Analyzers
{
    /// <summary>
    /// Analyzer that ensures migration attribute versions are unique within a compilation.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MigrationAttributeVersionShouldBeUniqueAnalyzer : FluentMigratorAnalyzer
    {
        /// <summary>
        /// The diagnostic ID for duplicate migration version warnings.
        /// </summary>
        public const string DiagnosticId = "FM0001";
        private const string Category = "FluentMigrator";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.MigrationAttributeVersionShouldBeUniqueAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.MigrationAttributeVersionShouldBeUniqueAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.MigrationAttributeVersionShouldBeUniqueAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description, customTags: "CompilationEnd");

        /// <summary>
        /// Gets the set of diagnostic descriptors supported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, FluentMigratorContext fluentMigratorContext)
        {
            // Get all migration classes and their version number
            compilationStartContext.RegisterSymbolAction(symbolContext =>
            {
                var typeSymbol = (INamedTypeSymbol)symbolContext.Symbol;
                var migrationVersion = GetMigrationAttributeVersion(fluentMigratorContext, typeSymbol.GetAttributes());
                if (migrationVersion == null)
                {
                    return;
                }

                fluentMigratorContext.Migrations.Add(new MigrationClassDeclaration(typeSymbol, migrationVersion.Value));

            }, SymbolKind.NamedType);

            // At the end: report duplicates
            compilationStartContext.RegisterCompilationEndAction(
                context =>
                {
                    var duplicates = fluentMigratorContext.Migrations
                        .OrderBy(m => m.Name)
                        .GroupBy(m => m.Version)
                        .Where(g => g.Count() > 1);

                    foreach (var duplicate in duplicates)
                    {
                        var groupName = string.Join(", ", duplicate.Select(d => d.Name));

                        foreach (var tuple in duplicate)
                        {
                            context.ReportDiagnostic(
                                Diagnostic.Create(
                                    Rule,
                                    tuple.Location,
                                    groupName,
                                    tuple.Version
                                )
                            );
                        }
                    }
                });
        }

        private static long? GetMigrationAttributeVersion(FluentMigratorContext fluentMigratorContext, IEnumerable<AttributeData> attributes)
            => attributes
                .FirstOrDefault(a => fluentMigratorContext.MigrationAttributeType.IsAssignableFrom(a.AttributeClass))
                ?.ConstructorArguments[0].Value as long?;
    }
}
