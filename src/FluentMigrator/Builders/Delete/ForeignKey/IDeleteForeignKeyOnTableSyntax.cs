using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete.ForeignKey
{
	public interface IDeleteForeignKeyOnTableSyntax : IFluentSyntax
	{
		void OnTable(string table);
	}
}
