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

using FluentMigrator.Analyzers.Analysis;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FluentMigrator.Analyzers
{
    /// <summary>
    /// Analyzer that warns when a legacy migration calls one of the
    /// <c>Execute.Sql(...)</c>, <c>Execute.Script(...)</c> or <c>Execute.EmbeddedScript(...)</c>
    /// overloads that take an <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/>
    /// of <see cref="string"/> to <see cref="string"/> parameter values, and recommends the
    /// <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/> of
    /// <see cref="string"/> to <see cref="object"/> overloads instead.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LegacyStringDictionaryParameterAnalyzer : FluentMigratorAnalyzer
    {
        /// <summary>
        /// The diagnostic ID for legacy <c>IDictionary&lt;string, string&gt;</c> parameter usage.
        /// </summary>
        public const string DiagnosticId = "FMUP0001";
        private const string Category = "FluentMigrator";

        internal static readonly ImmutableHashSet<string> ExecuteMethodNames =
            ImmutableHashSet.Create("Sql", "Script", "EmbeddedScript");

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.LegacyStringDictionaryParameterAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.LegacyStringDictionaryParameterAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.LegacyStringDictionaryParameterAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

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
            if (methodSymbol == null || !ExecuteMethodNames.Contains(methodSymbol.Name))
            {
                return;
            }

            if (!IsExecuteMethod(methodSymbol, executeExpressionRootType))
            {
                return;
            }

            var parameterIndex = GetStringDictionaryParameterIndex(methodSymbol);
            if (parameterIndex < 0)
            {
                return;
            }

            var argument = GetArgumentForParameter(invocation.ArgumentList, methodSymbol, parameterIndex);
            var location = (argument?.Expression ?? (SyntaxNode)invocation).GetLocation();

            context.ReportDiagnostic(Diagnostic.Create(Rule, location, methodSymbol.Name));
        }

        private static int GetStringDictionaryParameterIndex(IMethodSymbol methodSymbol)
        {
            for (var i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                if (IsStringDictionary(methodSymbol.Parameters[i].Type))
                {
                    return i;
                }
            }

            return -1;
        }

        internal static bool IsStringDictionary(ITypeSymbol type)
        {
            if (!(type is INamedTypeSymbol namedType) || !namedType.IsGenericType)
            {
                return false;
            }

            if (namedType.ConstructedFrom.ToDisplayString() != "System.Collections.Generic.IDictionary<TKey, TValue>")
            {
                return false;
            }

            return namedType.TypeArguments.Length == 2
                && namedType.TypeArguments[0].SpecialType == SpecialType.System_String
                && namedType.TypeArguments[1].SpecialType == SpecialType.System_String;
        }

        private static ArgumentSyntax GetArgumentForParameter(ArgumentListSyntax argumentList, IMethodSymbol methodSymbol, int parameterIndex)
        {
            if (argumentList == null)
            {
                return null;
            }

            var parameterName = methodSymbol.Parameters[parameterIndex].Name;

            var namedArgument = argumentList.Arguments
                .FirstOrDefault(a => a.NameColon?.Name.Identifier.ValueText == parameterName);
            if (namedArgument != null)
            {
                return namedArgument;
            }

            if (parameterIndex < argumentList.Arguments.Count
                && argumentList.Arguments[parameterIndex].NameColon == null)
            {
                return argumentList.Arguments[parameterIndex];
            }

            return null;
        }

        private static bool IsExecuteMethod(IMethodSymbol methodSymbol, INamedTypeSymbol executeExpressionRootType)
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
