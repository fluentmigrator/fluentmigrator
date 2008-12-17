using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Rename.Table
{
	public interface IRenameTableToNameSyntax : IFluentSyntax
	{
		void To(string name);
	}
}