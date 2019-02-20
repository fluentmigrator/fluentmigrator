using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Versioning;

namespace FluentMigrator.Runner.Constraints
{
    public class MigrationConstraintContext
    {
        public RunnerOptions RunnerOptions { get; set; }
        public IVersionInfo VersionInfo { get; set; }
    }
}
