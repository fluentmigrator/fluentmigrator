using System.IO;
using System.Linq;
using FluentMigrator;

namespace Migrations.FM_Extensions
{
    public abstract class MigrationExt : Migration
    {
        protected void DeleteType(string name)
        {
            // TODO: Will fail in MS-Access
            Execute.Sql(string.Format("DROP TYPE '{0}'", name));
        }

        protected void DeleteFunction(string name)
        {
            // TODO: Will fail in MS-Access
            Execute.Sql(string.Format("DROP FUNCTION '{0}'", name));
        }

        protected void DeleteStoredProcedure(string name)
        {
            Execute.Sql(string.Format("DROP PROCEDURE '{0}'", name));
        }

        protected void DeleteView(string name)
        {
            Execute.Sql(string.Format("DROP VIEW '{0}'", name));
        }

        protected void ExecuteScriptDirectory(DirectoryInfo sqlDir)
        {
            var files = from file in sqlDir.GetFiles("*.sql", SearchOption.AllDirectories)
                   orderby file.FullName // ensure a predictable execution order
                   select file;

            foreach (var file in files)
            {
                Execute.Script(file.FullName);
            }
        }

        protected void ExecuteScriptDirectory(string sqlDirPath)
        {
            ExecuteScriptDirectory(new DirectoryInfo(sqlDirPath));
        }
    }
}
