namespace FluentMigrator.VersionTableInfo
{
	public class DefaultVersionTableMetaData : IVersionTableMetaData
	{
		public string TableName
		{
			get { return "VersionInfo"; }
		}

		public string ColumnName
		{
			get { return "Version";}
		}
	}
}