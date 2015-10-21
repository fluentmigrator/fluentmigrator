namespace FluentMigrator
{
    public abstract class MigrationWithCheck : Migration
    {
        public override bool CheckIfExists { get { return true; } }

        public override void BeforeUp()
        {
            var migrationInfo = _context.Conventions.GetMigrationInfo(GetType());

            if (migrationInfo.DependsOn != default(long))
            {
                Execute.Sql(@"
                IF NOT EXISTS
                (
	                SELECT 1
	                FROM [dbo].[VersionInfo]
	                WHERE [Version] = " + migrationInfo.DependsOn + @"
                )
                BEGIN
                    RAISERROR('Dependend migration has not been run yet!', 16, 1)
	                RETURN
                END");
            }
        }
    }
}