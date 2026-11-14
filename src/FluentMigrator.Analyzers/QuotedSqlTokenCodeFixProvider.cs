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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

namespace FluentMigrator.Analyzers
{
    /// <summary>
    /// Replaces an automatically quoted token that sits inside a SQL string literal with the raw
    /// token style, which is what belongs there.
    /// </summary>
    /// <remarks>
    /// The alternative repair - deleting the surrounding quotes and keeping <c>$[name]</c> - is not
    /// offered automatically. Both produce valid SQL but they mean different things, and only the
    /// author knows whether the surrounding literal was intentional. Substituting into the existing
    /// literal is the change that preserves the statement as written.
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(QuotedSqlTokenCodeFixProvider)), Shared]
    public class QuotedSqlTokenCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(QuotedSqlTokenAnalyzer.DiagnosticId);

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
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: Resources.QuotedSqlTokenCodeFixTitle,
                        createChangedDocument: cancellationToken => ReplaceWithRawTokenAsync(context.Document, diagnosticSpan, cancellationToken),
                        equivalenceKey: nameof(QuotedSqlTokenCodeFixProvider)),
                    diagnostic);
            }
        }

        private static async Task<Document> ReplaceWithRawTokenAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var quotedTokenText = text.ToString(diagnosticSpan);
            var rawTokenText = QuotedSqlTokenAnalyzer.ToRawTokenText(quotedTokenText);

            return document.WithText(text.Replace(diagnosticSpan, rawTokenText));
        }
    }
}
