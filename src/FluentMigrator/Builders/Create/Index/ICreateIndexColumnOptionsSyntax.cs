using System;

namespace FluentMigrator.Builders.Create.Index
{
	public interface ICreateIndexColumnOptionsSyntax
	{
		ICreateIndexOnColumnSyntax Ascending();
		ICreateIndexOnColumnSyntax Descending();
	}
}