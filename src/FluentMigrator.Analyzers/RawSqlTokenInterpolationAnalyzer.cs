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
using System.Text.RegularExpressions;

using FluentMigrator.Analyzers.Analysis;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace FluentMigrator.Analyzers
{
    /// <summary>
    /// Analyzer that warns when a call to <c>Execute.Sql(...)</c> uses the raw, unquoted
    /// <c>$(name)</c> variable interpolation token style, and recommends the automatically quoted
    /// <c>$[name]</c> token style instead.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The escaped form <c>$$((name))</c> is deliberately <em>not</em> reported. It performs no
    /// substitution - it renders as the literal text <c>$(name)</c> - so it carries no injection
    /// risk, and rewriting it to <c>$$[[name]]</c> would change the emitted SQL from <c>$(name)</c>
    /// to <c>$[name]</c>.
    /// </para>
    /// <para>
    /// Tokens inside a comment are also not reported, since they are never executed.
    /// </para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RawSqlTokenInterpolationAnalyzer : FluentMigratorAnalyzer
    {
        /// <summary>
        /// The diagnostic ID for raw SQL token interpolation warnings.
        /// </summary>
        public const string DiagnosticId = "FM0002";
        private const string Category = "FluentMigrator";

        /// <summary>
        /// Matches the raw, unquoted token form <c>$(name)</c>.
        /// </summary>
        internal static readonly Regex RawTokenPattern = new Regex(@"\$\((?<token>\w+)\)", RegexOptions.Compiled);

        /// <summary>
        /// Diagnostic property set to <c>"true"</c> when the token is the entire content of a SQL
        /// string literal, so the code fix must remove the surrounding quotes as it rewrites.
        /// </summary>
        internal const string StripEnclosingQuotesProperty = "stripEnclosingQuotes";

        /// <summary>
        /// Diagnostic property set to <c>"true"</c> when the token is embedded in a larger SQL string
        /// literal. There is no mechanical rewrite in that case, so no code fix is offered.
        /// </summary>
        internal const string EmbeddedInLiteralProperty = "embeddedInLiteral";

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
            var matches = ExecuteSqlLiteral.MatchTokens(token, RawTokenPattern);
            if (matches.Count == 0)
            {
                return;
            }

            var dialect = SqlDialectResolver.Resolve(invocation, context.Options);
            var syntax = SqlLiteralSyntax.For(dialect);
            var sql = token.ValueText;

            foreach (var match in matches)
            {
                var region = SqlLiteralScanner.GetRegionAt(sql, match.SqlIndex, syntax);

                // Tokens in comments are never executed, so there is nothing to warn about.
                if (region.Kind == SqlRegionKind.Comment)
                {
                    continue;
                }

                ReportDiagnostic(context, token, match, region, sql);
            }
        }

        private static void ReportDiagnostic(
            SyntaxNodeAnalysisContext context,
            SyntaxToken token,
            SqlTokenMatch match,
            SqlRegion region,
            string sql)
        {
            var location = Location.Create(token.SyntaxTree, new TextSpan(match.SourceStart, match.SourceLength));
            var quotedTokenText = ToQuotedTokenText(match.Text);
            var properties = ImmutableDictionary<string, string>.Empty;

            if (IsStringLiteral(region.Kind))
            {
                // `'$(name)'` becomes `$[name]`: the token supplies the whole value, so the quotes
                // have to go with it. Rewriting the token alone would double-quote the value, which
                // is exactly what FM0003 reports.
                var contentStart = region.ContentStart;
                var contentEnd = FindContentEnd(sql, region);
                var tokenIsWholeLiteral = match.SqlIndex == contentStart &&
                                          match.SqlIndex + match.Text.Length == contentEnd;

                // Only single-quoted literals are safe to rewrite automatically. A SQL double quote
                // is written as `\"` or `""` in C# source, and Oracle's `q'[...]'` has a two-
                // character closer, so deleting delimiters by source offset would corrupt either.
                var canStripQuotes = tokenIsWholeLiteral && region.Kind == SqlRegionKind.StringLiteral;

                properties = canStripQuotes
                    ? properties.Add(StripEnclosingQuotesProperty, "true")
                    : properties.Add(EmbeddedInLiteralProperty, "true");
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, location, properties, match.Text, quotedTokenText));
        }

        private static bool IsStringLiteral(SqlRegionKind kind)
        {
            return kind == SqlRegionKind.StringLiteral ||
                   kind == SqlRegionKind.DoubleQuotedStringLiteral ||
                   kind == SqlRegionKind.AlternativeQuoted;
        }

        /// <summary>
        /// Finds the index just past the last content character of a literal region, i.e. the index
        /// of its closing delimiter.
        /// </summary>
        private static int FindContentEnd(string sql, SqlRegion region)
        {
            var end = region.End;
            if (end <= region.ContentStart)
            {
                return region.ContentStart;
            }

            // Closing delimiters are one character for quoted forms and two for Oracle's q'[ ]'.
            var closerLength = region.Kind == SqlRegionKind.AlternativeQuoted ? 2 : 1;
            var contentEnd = end - closerLength;

            return contentEnd < region.ContentStart ? region.ContentStart : contentEnd;
        }

        /// <summary>
        /// Converts a raw token (<c>$(name)</c>) into its automatically quoted equivalent
        /// (<c>$[name]</c>).
        /// </summary>
        internal static string ToQuotedTokenText(string rawTokenText)
        {
            return rawTokenText.Replace('(', '[').Replace(')', ']');
        }
    }
}
