using System.Data;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    internal class SqlServer2008TypeMap : SqlServer2005TypeMap
    {
        protected override void SetupTypeMaps()
        {
            base.SetupTypeMaps();

            SetTypeMap(DbType.DateTime2, "DATETIME2");
            SetTypeMap(DbType.DateTimeOffset, "DATETIMEOFFSET");
            SetTypeMap(DbType.Date, "DATE");
            SetTypeMap(DbType.Time, "TIME");
        }
    }
}
