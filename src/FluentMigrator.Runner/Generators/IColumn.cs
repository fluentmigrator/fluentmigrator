using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	public interface IColumn
	{
		string Generate(ColumnDefinition column);
		string Generate(CreateTableExpression expression);
	}
}
