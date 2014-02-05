using FluentMigrator;

namespace Migrations.FM_Extensions
{
    public abstract class MigrationExt : Migration
    {
        private void DeleteType(string name)
        {
            // TODO: Will fail in MS-Access
            Execute.Sql(string.Format("DROP TYPE '{0}'", name));
        }

        protected void DeleteFunction(string name)
        {
            // TODO: Will fail in MS-Access
            Execute.Sql(string.Format("DROP FUNCTION '{0}'", name));
        }

        private void DeleteStoredProcedure(string name)
        {
            Execute.Sql(string.Format("DROP PROCEDURE '{0}'", name));
        }

        protected void DeleteView(string name)
        {
            Execute.Sql(string.Format("DROP VIEW '{0}'", name));
        }
    }
}
