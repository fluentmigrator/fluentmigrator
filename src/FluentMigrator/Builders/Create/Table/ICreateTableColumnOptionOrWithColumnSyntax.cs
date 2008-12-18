using System;

namespace FluentMigrator.Builders.Create.Table
{
	public interface ICreateTableColumnOptionOrWithColumnSyntax : IColumnOptionSyntax<ICreateTableColumnOptionOrWithColumnSyntax>, ICreateTableWithColumnSyntax
	{
	}
}