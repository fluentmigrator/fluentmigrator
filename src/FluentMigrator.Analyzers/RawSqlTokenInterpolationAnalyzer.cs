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

using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

using FluentMigrator.Analyzers.Analysis;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FluentMigrator.Analyzers
{
    /// <summary>
    /// Analyzer that warns when a call to <c>Execute.Sql(...)</c> uses the raw, unquoted
    /// <c>$(name)</c>/<c>$$((name))</c> variable interpolation token style, and recommends the
    /// automatically quoted <c>$[name]</c>/<c>$$[[name]]</c> token style instead.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RawSqlTokenInterpolationAnalyzer : FluentMigratorAnalyzer
    {
        /// <summary>
        /// The diagnostic ID for raw SQL token interpolation warnings.
        /// </summary>
        public const string DiagnosticId = "FM0002";
        private const string Category = "FluentMigrator";

        /// <summary>
        /// Matches the escaped raw token form <c>$$((name))</c>, which is rendered verbatim (without
        /// substitution) as the literal text <c>$(name)</c>.
        /// </summary>
        internal static readonly Regex EscapedRawTokenPattern = new Regex(@"\$\$\(\((?<token>\w+)\)\)", RegexOptions.Compiled);

        /// <summary>
        /// Matches the raw, unquoted token form <c>$(name)</c>.
        /// </summary>
        internal static readonly Regex RawTokenPattern = new Regex(@"\$\((?<token>\w+)\)", RegexOptions.Compiled);

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.RawSqlTokenInterpolationAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.RawSqlTokenInterpolationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.RawSqlTokenInterpolationAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        /// <summary>
        /// Gets the set of diagnostic descriptors supported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, FluentMigratorContext fluentMigratorContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(
                symbolContext => AnalyzeInvocation(symbolContext, fluentMigratorContext),
                SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, FluentMigratorContext fluentMigratorContext)
        {
            var executeExpressionRootType = fluentMigratorContext.ExecuteExpressionRootType;
            if (executeExpressionRootType == null)
            {
                return;
            }

            var invocation = (InvocationExpressionSyntax)context.Node;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol as IMethodSymbol;
            if (methodSymbol == null || methodSymbol.Name != "Sql")
            {
                return;
            }

            if (!IsSqlExecuteMethod(methodSymbol, executeExpressionRootType))
            {
                return;
            }

            // The SQL statement is always the first argument of the `Sql(...)` overloads.
            var sqlArgument = invocation.ArgumentList?.Arguments.FirstOrDefault();
            if (!(sqlArgument?.Expression is LiteralExpressionSyntax literal) || !literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return;
            }

            var token = literal.Token;
            var tokenText = token.Text;

            foreach (Match match in EscapedRawTokenPattern.Matches(tokenText))
            {
                ReportDiagnostic(context, token, match);
            }

            foreach (Match match in RawTokenPattern.Matches(tokenText))
            {
                ReportDiagnostic(context, token, match);
            }
        }

        private static void ReportDiagnostic(SyntaxNodeAnalysisContext context, SyntaxToken token, Match match)
        {
            var location = Location.Create(token.SyntaxTree, new Microsoft.CodeAnalysis.Text.TextSpan(token.SpanStart + match.Index, match.Length));
            var quotedTokenText = ToQuotedTokenText(match.Value);

            context.ReportDiagnostic(Diagnostic.Create(Rule, location, match.Value, quotedTokenText));
        }

        /// <summary>
        /// Converts a raw token (<c>$(name)</c>) or escaped raw token (<c>$$((name))</c>) into its
        /// automatically quoted equivalent (<c>$[name]</c> or <c>$$[[name]]</c>, respectively).
        /// </summary>
        internal static string ToQuotedTokenText(string rawTokenText)
        {
            return rawTokenText.Replace('(', '[').Replace(')', ']');
        }

        private static bool IsSqlExecuteMethod(IMethodSymbol methodSymbol, INamedTypeSymbol executeExpressionRootType)
        {
            var containingType = methodSymbol.ContainingType;
            if (containingType == null)
            {
                return false;
            }

            if (SymbolEqualityComparer.Default.Equals(containingType, executeExpressionRootType))
            {
                return true;
            }

            return containingType.AllInterfaces.Contains(executeExpressionRootType, SymbolEqualityComparer.Default);
        }
    }
}
