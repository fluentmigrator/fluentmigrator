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
    /// Analyzer that warns when the automatically quoted <c>$[name]</c> token appears inside a SQL
    /// string literal, which quotes the value twice.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>$[name]</c> expands to a complete SQL literal - <c>'O''Brien'</c>, <c>NULL</c>, <c>1</c>,
    /// <c>0xdead</c>. Wrapping it in quotes therefore produces <c>''O''Brien''</c> rather than a
    /// string, and only the string case fails loudly:
    /// </para>
    /// <list type="table">
    /// <item><description><c>"O'Brien"</c> becomes <c>''O''Brien''</c> - a syntax error</description></item>
    /// <item><description><c>1234.5678</c> becomes <c>'1234.5678'</c> - silently a string, not a number</description></item>
    /// <item><description><see langword="null"/> becomes <c>'NULL'</c> - silently the four-character text</description></item>
    /// <item><description><c>RawSql</c> becomes <c>'CURRENT_TIMESTAMP'</c> - silently text, the function never runs</description></item>
    /// </list>
    /// <para>
    /// The raw <c>$(name)</c> style is the opposite: wrapping it in quotes is the correct and
    /// intended way to substitute a value into a string literal, so it is never reported here.
    /// </para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class QuotedSqlTokenAnalyzer : FluentMigratorAnalyzer
    {
        /// <summary>
        /// The diagnostic ID for double-quoted SQL token warnings.
        /// </summary>
        public const string DiagnosticId = "FM0003";
        private const string Category = "FluentMigrator";

        /// <summary>
        /// Matches the automatically quoted token form <c>$[name]</c>.
        /// </summary>
        internal static readonly Regex QuotedTokenPattern = new Regex(@"\$\[(?<token>\w+)\]", RegexOptions.Compiled);

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.QuotedSqlTokenAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.QuotedSqlTokenAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.QuotedSqlTokenAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        /// <summary>
        /// Gets the set of diagnostic descriptors supported by this analyzer.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, FluentMigratorContext fluentMigratorContext)
        {
            compilationStartContext.RegisterSyntaxNodeAction(
                syntaxContext => AnalyzeInvocation(syntaxContext, fluentMigratorContext),
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
            var matches = ExecuteSqlLiteral.MatchTokens(token, QuotedTokenPattern);
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

                if (!ShouldReport(region, syntax))
                {
                    continue;
                }

                var location = Location.Create(token.SyntaxTree, new TextSpan(match.SourceStart, match.SourceLength));
                var unquoted = ToRawTokenText(match.Text);

                context.ReportDiagnostic(Diagnostic.Create(Rule, location, match.Text, region.Opener, unquoted));
            }
        }

        /// <summary>
        /// Decides whether a token in <paramref name="region"/> is worth reporting.
        /// </summary>
        /// <remarks>
        /// Deliberately conservative in three places:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// PostgreSQL dollar-quoted blocks normally hold SQL or PL/pgSQL source rather than data, so
        /// a quoted literal inside one is usually correct.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// SQLite double quotes resolve to an identifier or a string depending on the schema, not on
        /// the text, so the outcome is not knowable at compile time.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// A double-quoted run is only a string literal in MySQL, and only without
        /// <c>ANSI_QUOTES</c>. It is therefore reported only when the dialect is known to be MySQL.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        private static bool ShouldReport(SqlRegion region, SqlLiteralSyntax syntax)
        {
            switch (region.Kind)
            {
                case SqlRegionKind.StringLiteral:
                case SqlRegionKind.AlternativeQuoted:
                    return true;

                case SqlRegionKind.DoubleQuotedStringLiteral:
                    return syntax.DoubleQuoteIsStringLiteral && !syntax.DoubleQuoteIsAmbiguous;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Converts an automatically quoted token (<c>$[name]</c>) into its raw equivalent
        /// (<c>$(name)</c>), which is what belongs inside a string literal.
        /// </summary>
        internal static string ToRawTokenText(string quotedTokenText)
        {
            return quotedTokenText.Replace('[', '(').Replace(']', ')');
        }
    }
}
