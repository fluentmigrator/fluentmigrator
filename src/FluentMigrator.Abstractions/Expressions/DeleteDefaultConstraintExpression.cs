using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    public class DeleteDefaultConstraintExpression : MigrationExpressionBase
    {
        public virtual string SchemaName { get; set; }
        public virtual string TableName { get; set; }
        public virtual string ColumnName { get; set; }

        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        public override void CollectValidationErrors(ICollection<string> errors)
        {
            if (String.IsNullOrEmpty(TableName))
                errors.Add(ErrorMessages.TableNameCannotBeNullOrEmpty);

            if (String.IsNullOrEmpty(ColumnName))
                errors.Add(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        public override string ToString()
        {
            return base.ToString() +
                   string.Format("{0}.{1} {2}",
                                 SchemaName,
                                 TableName,
                                 ColumnName);
        }
    }
}