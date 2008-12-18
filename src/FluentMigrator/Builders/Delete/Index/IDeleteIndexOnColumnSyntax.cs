using System;

namespace FluentMigrator.Builders.Delete.Index
{
	public interface IDeleteIndexOnColumnSyntax
	{
		void OnColumn(string columnName);
		void OnColumns(params string[] columnNames);
	}
}