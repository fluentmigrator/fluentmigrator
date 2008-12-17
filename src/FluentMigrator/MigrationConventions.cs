using System;
using FluentMigrator.Expressions;

namespace FluentMigrator
{
	public class MigrationConventions
	{
		public Func<CreateForeignKeyExpression, string> GetForeignKeyName { get; set; }
	}
}
