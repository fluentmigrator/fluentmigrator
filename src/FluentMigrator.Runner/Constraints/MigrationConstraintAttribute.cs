using System;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Versioning;

namespace FluentMigrator.Runner.Constraints
{
    /// <summary>
    /// Can be used to apply conditions when migrations will be run.
    /// </summary>
    public class MigrationConstraintAttribute : Attribute
    {
        private readonly Func<MigrationConstraintContext, bool> _predicate;

        public MigrationConstraintAttribute(Func<MigrationConstraintContext, bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException("predicate", "Predicate must not be null");
        }
        /// <summary>
        /// Determines whether the migration having this attribute should be run under given <paramref name="context">migration context</paramref>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>True if migration should be run under given <paramref name="context">migration context</paramref>.</returns>
        public bool ShouldRun(MigrationConstraintContext context)
        {
            return _predicate(context);
        }
    }
}
