using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	public interface IColumn
	{
		string Generate(ColumnDefinition column);
	}
}