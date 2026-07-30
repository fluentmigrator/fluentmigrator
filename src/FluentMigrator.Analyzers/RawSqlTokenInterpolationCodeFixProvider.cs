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
        /// Grows a token span to cover the SQL string literal that encloses it, including any
        /// literal prefix such as <c>N</c>, <c>E</c> or <c>_utf8mb4</c>.
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
        /// so this only has to find the delimiters on either side. If the surrounding text is not
        /// what was expected the original span is returned unchanged, leaving the previous - still
        /// conservative - behaviour.
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

            // Walk back over a literal prefix: N'...', E'...', U&'...', _utf8mb4'...'.
            var prefixStart = start;
            while (prefixStart > 0 && (char.IsLetterOrDigit(text[prefixStart - 1]) || text[prefixStart - 1] == '_' || text[prefixStart - 1] == '&'))
            {
                prefixStart--;
            }

            return TextSpan.FromBounds(prefixStart, end + 1);
        }
    }
}
