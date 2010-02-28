using System;
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
			this._context = context;
			this._tableName = tableName;
		}

		public bool Exists()
		{
			return this._context.QuerySchema.TableExists(this._tableName);
		}

		public ISchemaColumnSyntax Column(string columnName)
		{
			return new SchemaColumnQuery(this._tableName, columnName, this._context);
		}
	}
}