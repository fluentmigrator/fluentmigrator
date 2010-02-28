using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Schema.Column
{
	public class SchemaColumnQuery : ISchemaColumnSyntax
	{
		private readonly string _tableName;
		private readonly string _columnName;
		private readonly IMigrationContext _context;

		public SchemaColumnQuery(string tableName, string columnName, IMigrationContext context)
		{
			_tableName = tableName;
			_columnName = columnName;
			_context = context;
		}

		public bool Exists()
		{
			return _context.QuerySchema.ColumnExists(_tableName, _columnName);
		}
	}
}