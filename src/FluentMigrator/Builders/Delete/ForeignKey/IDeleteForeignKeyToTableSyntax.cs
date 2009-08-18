using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete.ForeignKey
{
	public interface IDeleteForeignKeyToTableSyntax : IFluentSyntax
	{
		void ToTable(string table);
	}
}