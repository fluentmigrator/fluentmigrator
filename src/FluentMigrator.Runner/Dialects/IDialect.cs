using System;
using System.Data;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Dialects
{
	public interface IDialect
	{
		void SetTypeMap(DbType type, string template);
		void SetTypeMap(DbType type, string template, int maxSize);
		string GetTypeMap(DbType type, int size, int precision);
		string GenerateDDLForColumn(ColumnDefinition column);
	}
}