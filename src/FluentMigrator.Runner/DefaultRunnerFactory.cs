using System.Collections.Generic;
using System.Reflection;
using FluentMigrator.Runner.Initialization;

namespace FluentMigrator.Runner
{
    /// <summary>Constructs implementations of FluentMigrator interfaces for the FluentMigrator runner.</summary>
    public class DefaultRunnerFactory : DefaultMigrationFactory, IRunnerFactory
    {
        /// <summary>Get an object which reads and writes database versions to the database.</summary>
        /// <param name="runner">The migration runner used to create the version table.</param>
        /// <param name="assembly">The assembly to scan for migrations.</param>
        /// <param name="conventions">The default rules for migration mappings.</param>
        public IVersionLoader GetVersionLoader(IMigrationRunner runner, Assembly assembly, IMigrationConventions conventions)
        {
            return new VersionLoader(runner, assembly, conventions);
        }

        /// <summary>Get a profile loader.</summary>
        /// <param name="runnerContext">The runner metadata.</param>
        /// <param name="runner">The runner which executes migrations.</param>
        /// <param name="conventions">The default rules for migration mappings.</param>
        public IProfileLoader GetProfileLoader(IRunnerContext runnerContext, IMigrationRunner runner, IMigrationConventions conventions)
        {
            return new ProfileLoader(runnerContext, runner, conventions);
        }

        /// <summary>Get a migration loader.</summary>
        /// <param name="conventions">The default rules for migration mappings.</param>
        /// <param name="assembly">The assembly to scan for migrations.</param>
        /// <param name="namespace">The namespace to scan for migrations.</param>
        /// <param name="tagsToMatch">The migration tags to match, or <c>null</c> to match all tags.</param>
        public IMigrationLoader GetMigrationLoader(IMigrationConventions conventions, Assembly assembly, string @namespace, IEnumerable<string> tagsToMatch)
        {
            return new MigrationLoader(conventions, assembly, @namespace, tagsToMatch);
        }
    }
}
