using System.Data;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    internal class SqlServer2005TypeMap : SqlServer2000TypeMap
    {
        protected override void SetupTypeMaps()
        {
            base.SetupTypeMaps();

            // SQL Server 2005+ should use VARCHAR(MAX), NVARCHAR(MAX) and VARBINARY(MAX) which replace TEXT and NTEXT 
            SetTypeMap(DbType.String, "NVARCHAR(MAX)", UnicodeTextCapacity);
            SetTypeMap(DbType.AnsiString, "VARCHAR(MAX)", AnsiTextCapacity);
            SetTypeMap(DbType.Binary, "VARBINARY(MAX)", ImageCapacity);

        }
    }
}
