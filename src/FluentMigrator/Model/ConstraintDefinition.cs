using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Model
{
    public enum ConstraintType
    {
        PrimaryKey,
        Unique
    }

    public class ConstraintDefinition : ICloneable, ICanBeConventional, ICanBeValidated
    {
        private ConstraintType constraintType;
        public bool IsPrimaryKeyConstraint { get { return ConstraintType.PrimaryKey == constraintType; } }
        public bool IsUniqueConstraint { get { return ConstraintType.Unique == constraintType; } }

        public string SchemaName { get; set; }
        public string ConstraintName { get; set; }
        public string TableName { get; set; }
        public ICollection<string> Columns = new HashSet<string>();


        /// <summary>
        /// Initializes a new instance of the <see cref="T:ConstraintDefinition"/> class.
        /// </summary>
        public ConstraintDefinition(ConstraintType type)
        {
            constraintType = type;
        }

        #region ICloneable Members

        public object Clone()
        {
            return new ConstraintDefinition(constraintType)
            {
                Columns = Columns,
                ConstraintName = ConstraintName,
                TableName = TableName
            };
        }

        #endregion

        #region ICanBeConventional Members

        public void ApplyConventions(IMigrationConventions conventions)
        {
            if (String.IsNullOrEmpty(ConstraintName)){ 
                ConstraintName = conventions.GetConstraintName(this);
            }
        }

        #endregion

        #region ICanBeValidated Members

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (string.IsNullOrEmpty(TableName))
            {
                errors.Add("Table name cannot be empty");
            }

            if (0 == Columns.Count)
            {
                errors.Add("At least one column must be specified");
            }
        }

        #endregion
    }
}
