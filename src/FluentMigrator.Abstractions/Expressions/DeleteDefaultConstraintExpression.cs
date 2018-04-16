using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to delete constraints
    /// </summary>
    public class DeleteDefaultConstraintExpression : MigrationExpressionBase, ISchemaExpression
    {
        /// <inheritdoc />
        public virtual string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the table name
        /// </summary>
        public virtual string TableName { get; set; }

        /// <summary>
        /// Gets or sets the column name
        /// </summary>
        public virtual string ColumnName { get; set; }

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        /// <inheritdoc />
        public override void CollectValidationErrors(ICollection<string> errors)
        {
            if (String.IsNullOrEmpty(TableName))
                errors.Add(ErrorMessages.TableNameCannotBeNullOrEmpty);

            if (String.IsNullOrEmpty(ColumnName))
                errors.Add(ErrorMessages.ColumnNameCannotBeNullOrEmpty);
        }

        /// <inheritdoc />
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
