using FluentMigrator.Builders.Schema.Table;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Schema
{
	public interface ISchemaExpressionRoot : IFluentSyntax
	{
		ISchemaTableSyntax Table(string tableName);
	}
}