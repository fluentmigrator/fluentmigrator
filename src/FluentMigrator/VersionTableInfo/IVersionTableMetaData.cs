using System;

namespace FluentMigrator.VersionTableInfo
{
	public interface IVersionTableMetaData
	{
		string TableName { get; }
		string ColumnName { get; }
	}
}