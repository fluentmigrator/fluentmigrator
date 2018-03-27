using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Infrastructure;
using FluentMigrator.Builders.Execute;

namespace FluentMigrator
{
    public abstract class AutoScriptMigration : MigrationBase
    {
        public sealed override void Up()
        {
            var root = new ExecuteExpressionRoot(_context);
            root.EmbeddedScript(_context.Conventions
                .GetAutoScriptUpName(GetType(),_context.QuerySchema.DatabaseType));
        }

        public sealed override void Down()
        {
            var root = new ExecuteExpressionRoot(_context);
            root.EmbeddedScript(_context.Conventions
                .GetAutoScriptDownName(GetType(), _context.QuerySchema.DatabaseType));
        }
    }
}
