using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete.ForeignKey
{
	public interface IDeleteForeignKeyPrimaryColumnSyntax : IFluentSyntax
	{
		void PrimaryColumn(string column);
		void PrimaryColumns(params string[] columns);
	}
}