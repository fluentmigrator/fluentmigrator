using System;

namespace FluentMigrator.Builders.Delete.Index
{
	public interface IDeleteIndexForTableSyntax
	{
		IDeleteIndexOnColumnSyntax OnTable(string tableName);
	}
}