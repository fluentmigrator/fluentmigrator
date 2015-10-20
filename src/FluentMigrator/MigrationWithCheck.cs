namespace FluentMigrator
{
    public abstract class MigrationWithCheck : Migration
    {
        public override bool CheckIfExists { get { return true; } }
    }
}