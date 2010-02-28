using System;
using FluentMigrator.Builders.Schema.Table;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Schema
{
	public class SchemaExpressionRoot : ISchemaExpressionRoot
	{
		private readonly IMigrationContext _context;

		public SchemaExpressionRoot(IMigrationContext context)
		{
			_context = context;
		}

		public ISchemaTableSyntax Table(string tableName)
		{
			return new SchemaTableQuery(_context, tableName);
		}
	}
}