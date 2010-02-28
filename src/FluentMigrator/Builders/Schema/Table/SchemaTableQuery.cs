using FluentMigrator.Builders.Schema.Column;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Schema.Table
{
	public class SchemaTableQuery : ISchemaTableSyntax
	{
		private readonly IMigrationContext _context;
		private readonly string _tableName;

		public SchemaTableQuery(IMigrationContext context, string tableName)
		{
			_context = context;
			_tableName = tableName;
		}

		public bool Exists()
		{
			return _context.QuerySchema.TableExists(_tableName);
		}

		public ISchemaColumnSyntax Column(string columnName)
		{
			return new SchemaColumnQuery(_tableName, columnName, _context);
		}
	}
}