using Microsoft.CodeAnalysis.Diagnostics;

namespace FluentMigrator.Analyzers
{
    public abstract class FluentMigratorAnalyzer : DiagnosticAnalyzer
    {
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(compilationStartContext =>
            {
                var fluentMigratorContext = new FluentMigratorContext(compilationStartContext.Compilation);
                AnalyzeCompilation(compilationStartContext, fluentMigratorContext);
            });
        }

        internal abstract void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, FluentMigratorContext fluentMigratorContext);
    }
}
