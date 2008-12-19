using System;
using FluentMigrator.Builders.Rename.Table;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Rename
{
	public interface IRenameExpressionRoot : IFluentSyntax
	{
		IRenameTableToSyntax Table(string oldName);
		IRenameColumnTableSyntax Column(string oldName);
	}
}