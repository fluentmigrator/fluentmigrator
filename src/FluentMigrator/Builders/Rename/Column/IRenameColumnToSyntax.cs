using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Rename.Column
{
	public interface IRenameColumnToSyntax : IFluentSyntax
	{
		void To(string name);
	}
}