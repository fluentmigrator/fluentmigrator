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
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentMigrator.Analyzers.Analysis;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace FluentMigrator.Analyzers
{
    /// <summary>
    /// Provides a code fix that replaces the raw <c>$(name)</c>/<c>$$((name))</c> SQL token
    /// interpolation forms with their automatically quoted <c>$[name]</c>/<c>$$[[name]]</c>
    /// equivalents.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RawSqlTokenInterpolationCodeFixProvider))]
    [Shared]
    public class RawSqlTokenInterpolationCodeFixProvider : CodeFixProvider
    {
        /// <summary>
        /// Gets the set of diagnostic IDs that this code fix provider can fix.
        /// </summary>
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RawSqlTokenInterpolationAnalyzer.DiagnosticId);

        /// <inheritdoc />
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc />
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return;
            }

            foreach (var diagnostic in context.Diagnostics)
            {
                // `'prefix$(name)suffix'` has no mechanical rewrite. Swapping in `$[name]` would
                // nest a quoted literal inside a quoted literal, which is the defect FM0003
                // reports, and deleting the surrounding quotes would change the statement. Report
                // only; the author has to decide.
                if (diagnostic.Properties.ContainsKey(RawSqlTokenInterpolationAnalyzer.EmbeddedInLiteralProperty))
                {
                    continue;
                }

                var diagnosticSpan = diagnostic.Location.SourceSpan;
                var stripEnclosingQuotes = diagnostic.Properties.ContainsKey(
                    RawSqlTokenInterpolationAnalyzer.StripEnclosingQuotesProperty);

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: Resources.RawSqlTokenInterpolationCodeFixTitle,
                        createChangedDocument: cancellationToken => ReplaceWithQuotedTokenAsync(
                            context.Document,
                            diagnosticSpan,
                            stripEnclosingQuotes,
                            cancellationToken),
                        equivalenceKey: nameof(RawSqlTokenInterpolationCodeFixProvider)),
                    diagnostic);
            }
        }

        private static async Task<Document> ReplaceWithQuotedTokenAsync(
            Document document,
            TextSpan diagnosticSpan,
            bool stripEnclosingQuotes,
            CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var rawTokenText = text.ToString(diagnosticSpan);
            var quotedTokenText = RawSqlTokenInterpolationAnalyzer.ToQuotedTokenText(rawTokenText);

            var replacementSpan = stripEnclosingQuotes
                ? ExpandOverEnclosingQuotes(text, diagnosticSpan)
                : diagnosticSpan;

            return document.WithText(text.Replace(replacementSpan, quotedTokenText));
        }

        /// <summary>
        /// Grows a token span to cover the SQL string literal that encloses it, including a
        /// recognised literal prefix such as <c>N</c>, <c>E</c>, <c>U&amp;</c> or <c>_utf8mb4</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>$[name]</c> already expands to a complete, quoted SQL literal, so converting
        /// <c>'$(name)'</c> has to delete the quotes as well. Rewriting the token alone produced
        /// <c>'$[name]'</c>, which double-quotes the value: numbers silently became strings,
        /// <see langword="null"/> silently became the text <c>'NULL'</c>, and only the string case
        /// failed loudly.
        /// </para>
        /// <para>
        /// The analyzer has already established that the token is the entire content of the literal,
        /// so this only has to find the delimiters on either side. The analyzer works on the string
        /// value while this walk works on the C# source, so a quote or prefix spelled with an escape
        /// sequence (<c>\'</c>, <c>\u004E</c>) is not matched here; in that case the original span
        /// is returned unchanged and only the token itself is rewritten, as before this method
        /// existed.
        /// </para>
        /// <para>
        /// Only a run that <see cref="SqlLiteralScanner.IsKnownLiteralPrefix"/> recognises is
        /// absorbed. Any other adjacent identifier belongs to the surrounding SQL rather than to
        /// the literal, so it is left in place and only the quotes are removed - otherwise
        /// <c>LIKE'$(name)'</c> would lose its <c>LIKE</c>.
        /// </para>
        /// </remarks>
        private static TextSpan ExpandOverEnclosingQuotes(SourceText text, TextSpan tokenSpan)
        {
            var start = tokenSpan.Start - 1;
            if (start < 0 || text[start] != '\'')
            {
                return tokenSpan;
            }

            var end = tokenSpan.End;
            if (end >= text.Length || text[end] != '\'')
            {
                return tokenSpan;
            }

            var spanWithoutPrefix = TextSpan.FromBounds(start, end + 1);

            // U&'...' is handled separately because '&' is not part of an identifier, so the
            // general walk below would stop at it. The character before the "U" must not be an
            // identifier character either: in FOO_U&'x' the "U" is the tail of the identifier
            // FOO_U and the ampersand an operator, not a Unicode-escape prefix. This mirrors
            // SqlLiteralScanner.ReadLiteralPrefixStart, which applies the same rule to the string
            // value; it cannot be shared directly because this walk reads the C# source.
            if (start >= 2 && text[start - 1] == '&' && (text[start - 2] == 'U' || text[start - 2] == 'u'))
            {
                var beforePrefix = start - 3;
                if (beforePrefix < 0 || !(char.IsLetterOrDigit(text[beforePrefix]) || text[beforePrefix] == '_'))
                {
                    return TextSpan.FromBounds(start - 2, end + 1);
                }

                return spanWithoutPrefix;
            }

            // Walk back over the whole adjacent identifier run, then absorb it only when the
            // scanner recognises it as a literal prefix. Consuming the entire run is what gives
            // the identifier-boundary rule for free: ColumnN'...' yields the candidate "ColumnN",
            // which is not a known prefix, so the column name survives.
            var prefixStart = start;
            while (prefixStart > 0 && (char.IsLetterOrDigit(text[prefixStart - 1]) || text[prefixStart - 1] == '_'))
            {
                prefixStart--;
            }

            if (prefixStart == start)
            {
                return spanWithoutPrefix;
            }

            var candidate = text.ToString(TextSpan.FromBounds(prefixStart, start));

            return SqlLiteralScanner.IsKnownLiteralPrefix(candidate)
                ? TextSpan.FromBounds(prefixStart, end + 1)
                : spanWithoutPrefix;
        }
    }
}
