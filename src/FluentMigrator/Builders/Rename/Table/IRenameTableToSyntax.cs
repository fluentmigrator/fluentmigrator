using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Rename.Table
{
	public interface IRenameTableToSyntax : IFluentSyntax
	{
		void To(string name);
	}
}