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
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace FluentMigrator.Analyzers
{
    /// <summary>
    /// Provides a code fix that upgrades a legacy <c>Dictionary&lt;string, string&gt;</c> /
    /// <c>IDictionary&lt;string, string&gt;</c> parameter map passed to
    /// <c>Execute.Sql(...)</c>, <c>Execute.Script(...)</c> or <c>Execute.EmbeddedScript(...)</c>
    /// to the recommended <c>Dictionary&lt;string, object&gt;</c> /
    /// <c>IDictionary&lt;string, object&gt;</c> form.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LegacyStringDictionaryParameterCodeFixProvider))]
    [Shared]
    public class LegacyStringDictionaryParameterCodeFixProvider : CodeFixProvider
    {
        private static readonly ImmutableHashSet<string> DictionaryTypeNames =
            ImmutableHashSet.Create("Dictionary", "IDictionary");

        /// <summary>
        /// Gets the set of diagnostic IDs that this code fix provider can fix.
        /// </summary>
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(LegacyStringDictionaryParameterAnalyzer.DiagnosticId);

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
                var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
                var expression = node as ExpressionSyntax ?? node.FirstAncestorOrSelf<ExpressionSyntax>();
                if (expression == null)
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: Resources.LegacyStringDictionaryParameterCodeFixTitle,
                        createChangedDocument: cancellationToken => UpgradeParameterDictionaryAsync(context.Document, expression, cancellationToken),
                        equivalenceKey: nameof(LegacyStringDictionaryParameterCodeFixProvider)),
                    diagnostic);
            }
        }

        private static async Task<Document> UpgradeParameterDictionaryAsync(Document document, ExpressionSyntax argumentExpression, CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);

            foreach (var genericName in CollectDictionaryTypeSyntaxes(argumentExpression, editor.SemanticModel, cancellationToken))
            {
                editor.ReplaceNode(genericName, WithObjectValueType(genericName));
            }

            return editor.GetChangedDocument();
        }

        private static IEnumerable<GenericNameSyntax> CollectDictionaryTypeSyntaxes(ExpressionSyntax argumentExpression, SemanticModel semanticModel, CancellationToken cancellationToken)
        {
            switch (argumentExpression)
            {
                case ObjectCreationExpressionSyntax objectCreation when TryGetDictionaryGenericName(objectCreation.Type, out var creationGenericName):
                    yield return creationGenericName;
                    break;

                case IdentifierNameSyntax identifier:
                    {
                        var symbol = semanticModel.GetSymbolInfo(identifier, cancellationToken).Symbol;
                        if (symbol == null)
                        {
                            break;
                        }

                        foreach (var reference in symbol.DeclaringSyntaxReferences)
                        {
                            if (!(reference.GetSyntax(cancellationToken) is VariableDeclaratorSyntax declarator))
                            {
                                continue;
                            }

                            if (declarator.Parent is VariableDeclarationSyntax declaration
                                && TryGetDictionaryGenericName(declaration.Type, out var declarationGenericName))
                            {
                                yield return declarationGenericName;
                            }

                            if (declarator.Initializer?.Value is ObjectCreationExpressionSyntax initializerCreation
                                && TryGetDictionaryGenericName(initializerCreation.Type, out var initializerGenericName))
                            {
                                yield return initializerGenericName;
                            }
                        }

                        break;
                    }
            }
        }

        private static bool TryGetDictionaryGenericName(TypeSyntax typeSyntax, out GenericNameSyntax genericName)
        {
            genericName = typeSyntax as GenericNameSyntax
                ?? (typeSyntax as QualifiedNameSyntax)?.Right as GenericNameSyntax;

            if (genericName == null || genericName.TypeArgumentList.Arguments.Count != 2)
            {
                genericName = null;
                return false;
            }

            if (!DictionaryTypeNames.Contains(genericName.Identifier.ValueText))
            {
                genericName = null;
                return false;
            }

            return true;
        }

        private static GenericNameSyntax WithObjectValueType(GenericNameSyntax genericName)
        {
            var arguments = genericName.TypeArgumentList.Arguments;
            var valueType = arguments[1];

            var objectType = SyntaxFactory
                .PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword))
                .WithTriviaFrom(valueType);

            var newArguments = arguments.Replace(valueType, objectType);
            return genericName.WithTypeArgumentList(genericName.TypeArgumentList.WithArguments(newArguments));
        }
    }
}
