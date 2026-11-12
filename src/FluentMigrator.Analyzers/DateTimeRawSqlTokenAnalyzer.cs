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
    /// and the code fix rewrites the token to <c>$[name]</c>, which formats the value through the provider's quoter
    /// (ISO 8601 on most providers, <c>to_date(...)</c> on Oracle). Oracle's <c>DateTimeOffset</c> is the exception:
    /// it is emitted as a quoted string with no conversion, so it still depends on <c>NLS_TIMESTAMP_TZ_FORMAT</c>.
    /// </para>
    /// <para>
    /// FM0002 reports every raw token as a possible injection risk, and projects that use raw tokens on
    /// purpose (for identifiers, say) often suppress it. This diagnostic is a correctness problem rather
    /// than a style preference, so it is a separate rule that stays on when FM0002 is suppressed.
    /// </para>
    /// <para>
    /// The analyzer sees the SQL and the parameters separately, and its fix rewrites SQL, so it reports only when
    /// it knows exactly what the token will receive:
    /// <list type="bullet">
    /// <item><description>the parameters are a <c>Dictionary&lt;string, ...&gt;</c> created in the call, or a local
    /// initialized with one that is never reassigned, modified or passed anywhere except <c>Execute.Sql</c>;</description></item>
    /// <item><description>every key in the initializer is a constant string; and</description></item>
    /// <item><description>the comparer is the default, <c>StringComparer.Ordinal</c> or
    /// <c>StringComparer.OrdinalIgnoreCase</c>. Culture-sensitive comparers depend on the runtime culture.</description></item>
    /// </list>
    /// Anything else, as well as <c>Execute.Script</c>/<c>EmbeddedScript</c> (whose SQL is not in the source), is
    /// not reported.
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

            var dateValues = GetDateValuedKeys(
                invocation,
                context.SemanticModel,
                fluentMigratorContext.ExecuteExpressionRootType,
                context.CancellationToken);
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
        /// <remarks>
        /// A diagnostic here comes with a fix that rewrites SQL, so this errs towards silence: any dictionary
        /// whose final contents or key semantics are not certain yields no keys at all.
        /// </remarks>
        private static Dictionary<string, string> GetDateValuedKeys(
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel,
            INamedTypeSymbol executeExpressionRootType,
            CancellationToken cancellationToken)
        {
            var none = new Dictionary<string, string>(StringComparer.Ordinal);

            // The SQL statement is the first argument; the parameters are one of the others.
            foreach (var argument in invocation.ArgumentList.Arguments.Skip(1))
            {
                var creation = ResolveDictionaryCreation(argument.Expression, semanticModel, executeExpressionRootType, cancellationToken);
                if (creation?.Initializer == null)
                {
                    continue;
                }

                // The token replacer looks keys up through the dictionary's own comparer, so the analysis must
                // use the same equality. Only comparers whose equality is known exactly are modelled.
                if (!IsGenericDictionary(semanticModel.GetTypeInfo(creation, cancellationToken).Type) ||
                    !TryGetKeyComparer(creation, semanticModel, cancellationToken, out var comparer))
                {
                    return none;
                }

                var entries = new Dictionary<string, string>(comparer);

                foreach (var (keyExpression, valueExpression) in GetEntries(creation.Initializer))
                {
                    // A key that isn't a constant string could overwrite any entry, including a date.
                    var key = semanticModel.GetConstantValue(keyExpression, cancellationToken);
                    if (!key.HasValue || !(key.Value is string keyText))
                    {
                        return none;
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

                return entries;
            }

            return none;
        }

        /// <summary>
        /// Gets the object creation that produces the parameters: the argument itself, or the initializer
        /// of a local variable whose contents cannot have changed by the time it is passed.
        /// </summary>
        private static BaseObjectCreationExpressionSyntax ResolveDictionaryCreation(
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            INamedTypeSymbol executeExpressionRootType,
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
                declarator.Initializer?.Value is BaseObjectCreationExpressionSyntax localCreation &&
                IsOnlyPassedToExecuteSql(local, declarator, semanticModel, executeExpressionRootType, cancellationToken))
            {
                return localCreation;
            }

            return null;
        }

        /// <summary>
        /// Determines whether every use of <paramref name="local"/> is as a plain argument to <c>Execute.Sql(...)</c>.
        /// </summary>
        /// <remarks>
        /// That rules out reassignment, indexer writes, <c>Add</c>/<c>Remove</c>, and passing the dictionary
        /// anywhere it could be changed, so the declaration's initializer is exactly what each call receives.
        /// </remarks>
        private static bool IsOnlyPassedToExecuteSql(
            ILocalSymbol local,
            VariableDeclaratorSyntax declarator,
            SemanticModel semanticModel,
            INamedTypeSymbol executeExpressionRootType,
            CancellationToken cancellationToken)
        {
            // A local is visible only inside its member; with top-level statements, inside the compilation unit.
            SyntaxNode scope = declarator.FirstAncestorOrSelf<MemberDeclarationSyntax>();
            if (scope == null || scope is GlobalStatementSyntax)
            {
                scope = declarator.SyntaxTree.GetRoot(cancellationToken);
            }

            foreach (var reference in scope.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (reference.Identifier.ValueText != local.Name ||
                    !SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(reference, cancellationToken).Symbol, local))
                {
                    continue;
                }

                if (!(reference.Parent is ArgumentSyntax argument) ||
                    !argument.RefKindKeyword.IsKind(SyntaxKind.None) ||
                    !(argument.Parent?.Parent is InvocationExpressionSyntax invocation) ||
                    !ExecuteSqlLiteral.IsExecuteSqlInvocation(invocation, semanticModel, executeExpressionRootType, cancellationToken))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsGenericDictionary(ITypeSymbol type)
        {
            return type is INamedTypeSymbol named &&
                   named.OriginalDefinition.MetadataName == "Dictionary`2" &&
                   named.OriginalDefinition.ContainingNamespace?.ToDisplayString() == "System.Collections.Generic";
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
        /// Gets the key equality of <c>new Dictionary&lt;string, ...&gt;(...)</c>: ordinal by default or with
        /// <c>StringComparer.Ordinal</c>, ordinal ignore-case with <c>StringComparer.OrdinalIgnoreCase</c>.
        /// </summary>
        /// <remarks>
        /// Culture-sensitive comparers are not modelled: under <c>tr-TR</c>, <c>CurrentCultureIgnoreCase</c> keeps
        /// <c>"i"</c> and <c>"I"</c> apart, and the analyzer cannot know the culture the migration runs under. For
        /// those, and for any comparer it cannot identify, this returns <see langword="false"/>.
        /// </remarks>
        private static bool TryGetKeyComparer(
            BaseObjectCreationExpressionSyntax creation,
            SemanticModel semanticModel,
            CancellationToken cancellationToken,
            out StringComparer comparer)
        {
            comparer = StringComparer.Ordinal;

            var arguments = creation.ArgumentList?.Arguments ?? default;
            if (arguments.Count == 0)
            {
                return true;
            }

            if (arguments.Count > 1)
            {
                return false;
            }

            var expression = arguments[0].Expression;

            // new Dictionary<string, object>(capacity)
            if (semanticModel.GetTypeInfo(expression, cancellationToken).Type?.SpecialType == SpecialType.System_Int32)
            {
                return true;
            }

            if (semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol is IPropertySymbol property &&
                property.ContainingType?.Name == nameof(StringComparer) &&
                property.ContainingType.ContainingNamespace?.ToDisplayString() == "System")
            {
                switch (property.Name)
                {
                    case nameof(StringComparer.Ordinal):
                        return true;
                    case nameof(StringComparer.OrdinalIgnoreCase):
                        comparer = StringComparer.OrdinalIgnoreCase;
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
