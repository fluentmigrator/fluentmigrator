using System;
using FluentMigrator.Infrastructure;
using FluentMigrator.Processors;

namespace FluentMigrator.Expressions
{
	public interface IMigrationExpression : ICanBeValidated
	{
		void ExecuteWith(IMigrationProcessor processor);
	}
}