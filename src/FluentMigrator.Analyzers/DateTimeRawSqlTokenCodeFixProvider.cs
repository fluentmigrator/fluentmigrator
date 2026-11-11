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
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace FluentMigrator.Analyzers
{
    /// <summary>
    /// Rewrites a raw <c>$(name)</c> token that receives a date/time value to <c>$[name]</c>, removing the
    /// SQL quotes around it when the token was the whole literal (<c>'$(cutoff)'</c> becomes <c>$[cutoff]</c>).
    /// </summary>
    /// <remarks>
    /// The rewrite is the same one FM0002 offers, so this delegates to
    /// <see cref="RawSqlTokenInterpolationCodeFixProvider"/>. When the token is embedded in a larger literal,
    /// such as <c>'$(day) 00:00'</c>, no fix is offered: there is no mechanical rewrite.
    /// </remarks>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DateTimeRawSqlTokenCodeFixProvider))]
    [Shared]
    public class DateTimeRawSqlTokenCodeFixProvider : CodeFixProvider
    {
        /// <inheritdoc />
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DateTimeRawSqlTokenAnalyzer.DiagnosticId);

        /// <inheritdoc />
        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc />
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Properties.ContainsKey(RawSqlTokenInterpolationAnalyzer.EmbeddedInLiteralProperty))
                {
                    continue;
                }

                var diagnosticSpan = diagnostic.Location.SourceSpan;
                var stripEnclosingQuotes = diagnostic.Properties.ContainsKey(
                    RawSqlTokenInterpolationAnalyzer.StripEnclosingQuotesProperty);

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: Resources.DateTimeRawSqlTokenCodeFixTitle,
                        createChangedDocument: cancellationToken => RawSqlTokenInterpolationCodeFixProvider.ReplaceWithQuotedTokenAsync(
                            context.Document,
                            diagnosticSpan,
                            stripEnclosingQuotes,
                            cancellationToken),
                        equivalenceKey: nameof(DateTimeRawSqlTokenCodeFixProvider)),
                    diagnostic);
            }

            return Task.CompletedTask;
        }
    }
}
