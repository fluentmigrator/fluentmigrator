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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using FluentMigrator.Analyzers.Analysis;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace FluentMigrator.Analyzers
{
    /// <summary>
    /// Warns when a raw <c>$(name)</c> token in <c>Execute.Sql(...)</c> receives a
    /// <see cref="DateTime"/> or <see cref="DateTimeOffset"/> value (#2354).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>$(name)</c> renders the value's invariant <c>ToString()</c>, <c>07/28/2026 13:45:06</c>, which
    /// databases interpret differently. FluentMigrator deliberately keeps that output, because changing it
    /// would silently change the SQL of existing migrations. This analyzer points at the call sites instead,
    /// and the code fix rewrites the token to <c>$[name]</c>, which renders an ISO 8601 literal.
    /// </para>
    /// <para>
    /// FM0002 reports every raw token as a possible injection risk, and projects that use raw tokens on
    /// purpose (for identifiers, say) often suppress it. This diagnostic is a correctness problem rather
    /// than a style preference, so it is a separate rule that stays on when FM0002 is suppressed.
    /// </para>
    /// <para>
    /// The analyzer sees the SQL and the parameters separately, so it reports only when it can correlate
    /// them: the parameters must be a dictionary created in the call, or a local variable initialized with
    /// one, with constant keys. Values added later, dictionaries passed in from elsewhere, and
    /// <c>Execute.Script</c>/<c>EmbeddedScript</c> (whose SQL is not in the source) are not analyzed.
    /// </para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DateTimeRawSqlTokenAnalyzer : FluentMigratorAnalyzer
    {
        /// <summary>
        /// The diagnostic ID. FM0004 and FM0005 are reserved by <c>adr/proposed/AsyncAllTheWay.md</c>, and
        /// FM0006 by <c>adr/proposed/ParallelAsyncAllTheWay.md</c>.
        /// </summary>
        public const string DiagnosticId = "FM0007";
        private const string Category = "FluentMigrator";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.DateTimeRawSqlTokenAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.DateTimeRawSqlTokenAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.DateTimeRawSqlTokenAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        /// <summary>
        /// Gets the set of diagnostic descriptors supported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, FluentMigratorContext fluentMigratorContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(
                context => AnalyzeInvocation(context, fluentMigratorContext),
                SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, FluentMigratorContext fluentMigratorContext)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;

            if (!ExecuteSqlLiteral.TryGetSqlLiteral(
                    invocation,
                    context.SemanticModel,
                    fluentMigratorContext.ExecuteExpressionRootType,
                    context.CancellationToken,
                    out var literal))
            {
                return;
            }

            var token = literal.Token;
            var matches = ExecuteSqlLiteral.MatchTokens(token, RawSqlTokenInterpolationAnalyzer.RawTokenPattern);
            if (matches.Count == 0)
            {
                return;
            }

            var dateValues = GetDateValuedKeys(invocation, context.SemanticModel, context.CancellationToken);
            if (dateValues.Count == 0)
            {
                return;
            }

            var dialect = SqlDialectResolver.Resolve(invocation, context.Options);
            var syntax = SqlLiteralSyntax.For(dialect);
            var sql = token.ValueText;

            foreach (var match in matches)
            {
                var name = match.Text.Substring(2, match.Text.Length - 3);
                if (!dateValues.TryGetValue(name, out var typeName))
                {
                    continue;
                }

                var region = SqlLiteralScanner.GetRegionAt(sql, match.SqlIndex, syntax);
                if (region.Kind == SqlRegionKind.Comment)
                {
                    continue;
                }

                var location = Location.Create(token.SyntaxTree, new TextSpan(match.SourceStart, match.SourceLength));
                var properties = RawSqlTokenInterpolationAnalyzer.GetCodeFixProperties(match, region, sql);
                var quotedTokenText = RawSqlTokenInterpolationAnalyzer.ToQuotedTokenText(match.Text);

                context.ReportDiagnostic(Diagnostic.Create(Rule, location, properties, match.Text, typeName, quotedTokenText));
            }
        }

        /// <summary>
        /// Maps each parameter key whose value is statically a date/time type to that type's name.
        /// </summary>
        private static Dictionary<string, string> GetDateValuedKeys(
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);

            // The SQL statement is the first argument; the parameters are one of the others.
            foreach (var argument in invocation.ArgumentList.Arguments.Skip(1))
            {
                var creation = ResolveDictionaryCreation(argument.Expression, semanticModel, cancellationToken);
                if (creation?.Initializer == null)
                {
                    continue;
                }

                var ignoreCase = UsesIgnoreCaseComparer(creation, semanticModel, cancellationToken);
                var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
                var entries = new Dictionary<string, string>(comparer);

                foreach (var (keyExpression, valueExpression) in GetEntries(creation.Initializer))
                {
                    var key = semanticModel.GetConstantValue(keyExpression, cancellationToken);
                    if (!key.HasValue || !(key.Value is string keyText))
                    {
                        continue;
                    }

                    var typeName = GetDateTypeName(semanticModel.GetTypeInfo(valueExpression, cancellationToken).Type);
                    if (typeName == null)
                    {
                        // A later entry for the same key wins at runtime, so forget an earlier date.
                        entries.Remove(keyText);
                    }
                    else
                    {
                        entries[keyText] = typeName;
                    }
                }

                // Token lookup follows the dictionary's comparer, so copy that into the result.
                return new Dictionary<string, string>(entries, comparer);
            }

            return result;
        }

        /// <summary>
        /// Gets the object creation that produces the parameters: the argument itself, or the initializer
        /// of the local variable it names.
        /// </summary>
        private static BaseObjectCreationExpressionSyntax ResolveDictionaryCreation(
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (expression is BaseObjectCreationExpressionSyntax creation)
            {
                return creation;
            }

            if (expression is IdentifierNameSyntax identifier &&
                semanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol is ILocalSymbol local &&
                local.DeclaringSyntaxReferences.Length == 1 &&
                local.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken) is VariableDeclaratorSyntax declarator &&
                declarator.SyntaxTree == semanticModel.SyntaxTree &&
                declarator.Initializer?.Value is BaseObjectCreationExpressionSyntax localCreation)
            {
                return localCreation;
            }

            return null;
        }

        private static IEnumerable<(ExpressionSyntax Key, ExpressionSyntax Value)> GetEntries(InitializerExpressionSyntax initializer)
        {
            foreach (var expression in initializer.Expressions)
            {
                switch (expression)
                {
                    // { ["key"] = value }
                    case AssignmentExpressionSyntax assignment
                        when assignment.Left is ImplicitElementAccessSyntax elementAccess &&
                             elementAccess.ArgumentList.Arguments.Count == 1:
                        yield return (elementAccess.ArgumentList.Arguments[0].Expression, assignment.Right);
                        break;

                    // { { "key", value } }
                    case InitializerExpressionSyntax element
                        when element.IsKind(SyntaxKind.ComplexElementInitializerExpression) &&
                             element.Expressions.Count == 2:
                        yield return (element.Expressions[0], element.Expressions[1]);
                        break;
                }
            }
        }

        /// <summary>
        /// Detects <c>new Dictionary&lt;string, object&gt;(StringComparer.OrdinalIgnoreCase)</c> and friends.
        /// </summary>
        private static bool UsesIgnoreCaseComparer(
            BaseObjectCreationExpressionSyntax creation,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (creation.ArgumentList == null)
            {
                return false;
            }

            foreach (var argument in creation.ArgumentList.Arguments)
            {
                if (semanticModel.GetSymbolInfo(argument.Expression, cancellationToken).Symbol is IPropertySymbol property &&
                    property.ContainingType?.Name == nameof(StringComparer) &&
                    property.ContainingType.ContainingNamespace?.ToDisplayString() == "System" &&
                    property.Name.EndsWith("IgnoreCase", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns <c>DateTime</c> or <c>DateTimeOffset</c> (unwrapping <see cref="Nullable{T}"/>), or
        /// <see langword="null"/> for any other type.
        /// </summary>
        /// <remarks>
        /// <c>DateOnly</c> has the same <c>MM/dd/yyyy</c> problem, but it is deliberately not reported yet:
        /// the quoters do not format it, so <c>$[name]</c> would not fix it.
        /// </remarks>
        private static string GetDateTypeName(ITypeSymbol type)
        {
            if (type is INamedTypeSymbol named &&
                named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
                named.TypeArguments.Length == 1)
            {
                type = named.TypeArguments[0];
            }

            if (type == null)
            {
                return null;
            }

            if (type.SpecialType == SpecialType.System_DateTime)
            {
                return nameof(DateTime);
            }

            if (type.Name == nameof(DateTimeOffset) && type.ContainingNamespace?.ToDisplayString() == "System")
            {
                return nameof(DateTimeOffset);
            }

            return null;
        }
    }
}
