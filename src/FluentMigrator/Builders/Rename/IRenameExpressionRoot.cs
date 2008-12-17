using System;
using FluentMigrator.Builders.Rename.Table;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Rename
{
	public interface IRenameExpressionRoot : IFluentSyntax
	{
		IRenameTableToNameSyntax Table(string oldName);
	}
}