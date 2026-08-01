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
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentMigrator.Analyzers.Analysis
{
    /// <summary>
    /// A matched substitution token, carrying both its position in the SQL text and its span in the
    /// C# source.
    /// </summary>
    internal readonly struct SqlTokenMatch
    {
        public SqlTokenMatch(string text, int sqlIndex, int sourceStart, int sourceLength)
        {
            Text = text;
            SqlIndex = sqlIndex;
            SourceStart = sourceStart;
            SourceLength = sourceLength;
        }

        /// <summary>Gets the matched token, for example <c>$[name]</c>.</summary>
        public string Text { get; }

        /// <summary>Gets the index of the token within the SQL statement.</summary>
        public int SqlIndex { get; }

        /// <summary>Gets the start of the token within the C# source file.</summary>
        public int SourceStart { get; }

        /// <summary>Gets the length of the token within the C# source file.</summary>
        public int SourceLength { get; }
    }

    /// <summary>
    /// Locates <c>Execute.Sql("...")</c> invocations and the substitution tokens inside them.
    /// </summary>
    internal static class ExecuteSqlLiteral
    {
        /// <summary>
        /// Tries to get the string literal argument of an <c>Execute.Sql(...)</c> invocation.
        /// </summary>
        /// <param name="invocation">The candidate invocation</param>
        /// <param name="semanticModel">The semantic model</param>
        /// <param name="executeExpressionRootType">The <c>IExecuteExpressionRoot</c> symbol</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <param name="literal">The SQL string literal, when matched</param>
        /// <returns><see langword="true"/> when the invocation is an <c>Execute.Sql(...)</c> call with a literal argument</returns>
        public static bool TryGetSqlLiteral(
            InvocationExpressionSyntax invocation,
            SemanticModel semanticModel,
            INamedTypeSymbol executeExpressionRootType,
            System.Threading.CancellationToken cancellationToken,
            out LiteralExpressionSyntax literal)
        {
            literal = null;

            if (executeExpressionRootType == null)
            {
                return false;
            }

            if (!(semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is IMethodSymbol methodSymbol) ||
                methodSymbol.Name != "Sql")
            {
                return false;
            }

            if (!IsSqlExecuteMethod(methodSymbol, executeExpressionRootType))
            {
                return false;
            }

            // The SQL statement is always the first argument of the `Sql(...)` overloads.
            var sqlArgument = invocation.ArgumentList?.Arguments.FirstOrDefault();
            if (!(sqlArgument?.Expression is LiteralExpressionSyntax candidate) ||
                !candidate.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return false;
            }

            literal = candidate;
            return true;
        }

        /// <summary>
        /// Matches <paramref name="pattern"/> against the SQL a literal actually produces, while
        /// reporting spans in the C# source.
        /// </summary>
        /// <param name="token">The C# string literal token</param>
        /// <param name="pattern">The token pattern to match</param>
        /// <returns>The matches, in order</returns>
        /// <remarks>
        /// <para>
        /// Matching runs against <see cref="SyntaxToken.ValueText"/> so that the scanner sees the SQL
        /// as the runtime will, rather than the C# source with its own quoting and escapes. Spans are
        /// recovered by matching the same pattern against <see cref="SyntaxToken.Text"/> and pairing
        /// the two sequences positionally.
        /// </para>
        /// <para>
        /// The pairing holds because substitution tokens contain no character that C# escaping can
        /// introduce or remove. If the two match counts ever disagree - which needs something like
        /// <c>"$[name]"</c> - no matches are reported rather than risk pointing a diagnostic at
        /// the wrong span.
        /// </para>
        /// </remarks>
        public static IReadOnlyList<SqlTokenMatch> MatchTokens(SyntaxToken token, Regex pattern)
        {
            var sqlMatches = pattern.Matches(token.ValueText);
            var sourceMatches = pattern.Matches(token.Text);

            if (sqlMatches.Count != sourceMatches.Count)
            {
                return new SqlTokenMatch[0];
            }

            var results = new List<SqlTokenMatch>(sqlMatches.Count);
            for (var i = 0; i < sqlMatches.Count; i++)
            {
                results.Add(new SqlTokenMatch(
                    sqlMatches[i].Value,
                    sqlMatches[i].Index,
                    token.SpanStart + sourceMatches[i].Index,
                    sourceMatches[i].Length));
            }

            return results;
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
