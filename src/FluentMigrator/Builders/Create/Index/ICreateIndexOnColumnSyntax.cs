using System;

namespace FluentMigrator.Builders.Create.Index
{
	public interface ICreateIndexOnColumnSyntax
	{
		ICreateIndexColumnOptionsSyntax OnColumn(string columnName);
	    ICreateIndexOptionsSyntax WithOptions();
	}
}