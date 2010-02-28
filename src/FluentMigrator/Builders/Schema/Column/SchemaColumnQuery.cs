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
			this._tableName = tableName;
			this._columnName = columnName;
			this._context = context;
		}

		public bool Exists()
		{
			return this._context.QuerySchema.ColumnExists(this._tableName, this._columnName);
		}
	}
}