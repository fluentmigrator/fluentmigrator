using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Tests.Unit
{
	[VersionTableMetaData]
	public class TestVersionTableMetaData:IVersionTableMetaData 
	{
		public const string TABLENAME = "testTableName";
		public const string COLUMNNAME = "testColumnName";

		public string TableName
		{
			get { return TABLENAME; }
		}

		public string ColumnName
		{
			get { return COLUMNNAME; }
		}
	}
}
