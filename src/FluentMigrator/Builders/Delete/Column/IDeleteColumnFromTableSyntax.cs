using System;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Delete.Column
{
	public interface IDeleteColumnFromTableSyntax : IFluentSyntax
	{
		void FromTable(string tableName);
	}
}