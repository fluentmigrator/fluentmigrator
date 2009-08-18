using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Create.ForeignKey
{
	public interface ICreateForeignKeyPrimaryColumnSyntax : IFluentSyntax
	{
		void PrimaryColumn(string column);
		void PrimaryColumns(params string[] columns);
	}
}