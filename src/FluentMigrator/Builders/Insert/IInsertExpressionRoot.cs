using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Insert
{
	public interface IInsertExpressionRoot: IFluentSyntax
	{
		IInsertDataSyntax IntoTable(string tableName);
	}
}