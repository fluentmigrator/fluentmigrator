using System;
using System.Collections.Generic;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to alter default constraints
    /// </summary>
    public class AlterDefaultConstraintExpression : MigrationExpressionBase,
        ISchemaExpression
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

        /// <summary>
        /// Gets or sets the default value
        /// </summary>
        public virtual object DefaultValue { get; set; }

        /// <inheritdoc />
        public override void CollectValidationErrors(ICollection<string> errors)
        {
            if (String.IsNullOrEmpty(TableName))
                errors.Add(ErrorMessages.TableNameCannotBeNullOrEmpty);

            if (String.IsNullOrEmpty(ColumnName))
                errors.Add(ErrorMessages.ColumnNameCannotBeNullOrEmpty);

            if(DefaultValue == null)
                errors.Add(ErrorMessages.DefaultValueCannotBeNull);
        }

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() +
                    string.Format("{0}.{1} {2} {3}",
                                SchemaName,
                                TableName,
                                ColumnName,
                                DefaultValue);
        }
    }
}
