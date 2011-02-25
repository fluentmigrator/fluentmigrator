

namespace FluentMigrator.Runner.Generators.SqlServer
{
    using System.Data;

	internal class SqlServer2005TypeMap : SqlServer2000TypeMap
	{
		protected override void SetupTypeMaps()
		{
			base.SetupTypeMaps();

			SetTypeMap(DbType.String, "NVARCHAR(MAX)", UnicodeTextCapacity);
			SetTypeMap(DbType.AnsiString, "VARCHAR(MAX)", AnsiTextCapacity);
			SetTypeMap(DbType.Binary, "VARBINARY(MAX)", ImageCapacity);

		}
	}
}
