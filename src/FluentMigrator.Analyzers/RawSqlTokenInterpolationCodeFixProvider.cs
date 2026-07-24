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
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: Resources.RawSqlTokenInterpolationCodeFixTitle,
                        createChangedDocument: cancellationToken => ReplaceWithQuotedTokenAsync(context.Document, diagnosticSpan, cancellationToken),
                        equivalenceKey: nameof(RawSqlTokenInterpolationCodeFixProvider)),
                    diagnostic);
            }
        }

        private static async Task<Document> ReplaceWithQuotedTokenAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var rawTokenText = text.ToString(diagnosticSpan);
            var quotedTokenText = RawSqlTokenInterpolationAnalyzer.ToQuotedTokenText(rawTokenText);

            var newText = text.Replace(diagnosticSpan, quotedTokenText);
            return document.WithText(newText);
        }
    }
}
