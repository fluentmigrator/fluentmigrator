using System;
using System.Linq;
using FluentMigrator.Infrastructure;

namespace FluentMigrator
{
	public abstract class AutoReversingMigration : Migration
	{
		public sealed override void Down()
		{
		}

		public override void GetDownExpressions(IMigrationContext context)
		{
			GetUpExpressions(context);
			context.Expressions = context.Expressions.Select(e => e.Reverse()).Reverse().ToList();
		}
	}
}
