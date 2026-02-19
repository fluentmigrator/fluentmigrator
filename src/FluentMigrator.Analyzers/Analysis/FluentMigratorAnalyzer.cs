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

using Microsoft.CodeAnalysis.Diagnostics;

namespace FluentMigrator.Analyzers.Analysis
{
    /// <summary>
    /// Base class for FluentMigrator analyzers.
    /// </summary>
    public abstract class FluentMigratorAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Initializes the analyzer by registering actions for compilation analysis.
        /// </summary>
        /// <param name="context">The analysis context.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var fluentMigratorContext = new FluentMigratorContext(compilationStartContext.Compilation);
                AnalyzeCompilation(compilationStartContext, fluentMigratorContext);
            });
        }

        internal abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, FluentMigratorContext fluentMigratorContext);
    }
}
