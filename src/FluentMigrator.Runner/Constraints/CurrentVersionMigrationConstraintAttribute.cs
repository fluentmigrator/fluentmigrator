namespace FluentMigrator.Runner.Constraints
{
    /// <summary>
    /// Specifies minimum schema version against which this migration will be run.
    /// </summary>
    public class CurrentVersionMigrationConstraintAttribute : MigrationConstraintAttribute
    {
        /// <param name="minimumVersionToRunAgainst">The schema must equal or greater to this value for this migration to be run.</param>
        public CurrentVersionMigrationConstraintAttribute(long minimumVersionToRunAgainst)
            : base(ctx => ctx.VersionInfo.Latest() >= minimumVersionToRunAgainst) {
        }

    }
}
