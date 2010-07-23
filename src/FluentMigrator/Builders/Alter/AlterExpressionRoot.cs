using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Builders.Alter.Column;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Alter
{
    public class AlterExpressionRoot : IAlterExpressionRoot
    {
        private readonly IMigrationContext _context;

        public AlterExpressionRoot(IMigrationContext context)
		{
			_context = context;
		}

        //public void Schema(string schemaName)
        //{
        //    var expression = new AlterSchemaExpression { SchemaName = schemaName };
        //    _context.Expressions.Add(expression);
        //}

        public IAlterColumnOnTableSyntax Column(string columnName)
        {
            var expression = new AlterColumnExpression { Column = { Name = columnName } };
            _context.Expressions.Add(expression);
            return new AlterColumnExpressionBuilder(expression, _context);
        }
    }
}
