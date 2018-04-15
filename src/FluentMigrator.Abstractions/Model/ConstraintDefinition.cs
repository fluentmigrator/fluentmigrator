using System;
using System.Collections.Generic;

using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;

namespace FluentMigrator.Model
{
    public enum ConstraintType
    {
        PrimaryKey,
        Unique
    }

    public class ConstraintDefinition : ICloneable, ICanBeValidated, ISupportAdditionalFeatures
    {
        private ConstraintType constraintType;
        public bool IsPrimaryKeyConstraint { get { return ConstraintType.PrimaryKey == constraintType; } }
        public bool IsUniqueConstraint { get { return ConstraintType.Unique == constraintType; } }

        public virtual string SchemaName { get; set; }
        public virtual string ConstraintName { get; set; }
        public virtual string TableName { get; set; }
        public virtual ICollection<string> Columns { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="T:ConstraintDefinition"/> class.
        /// </summary>
        public ConstraintDefinition(ConstraintType type)
        {
            constraintType = type;

            Columns = new HashSet<string>();
        }

        public IDictionary<string, object> AdditionalFeatures { get; } = new Dictionary<string, object>();

        #region ICloneable Members

        public object Clone()
        {
            var result = new ConstraintDefinition(constraintType)
            {
                Columns = Columns,
                ConstraintName = ConstraintName,
                TableName = TableName
            };

            AdditionalFeatures.CloneTo(result.AdditionalFeatures);

            return result;
        }

        #endregion

        #region ICanBeValidated Members

        public void CollectValidationErrors(ICollection<string> errors)
        {
            if (string.IsNullOrEmpty(TableName))
            {
                errors.Add(ErrorMessages.TableNameCannotBeNullOrEmpty);
            }

            if (0 == Columns.Count)
            {
                errors.Add(ErrorMessages.ConstraintMustHaveAtLeastOneColumn);
            }
        }

        #endregion
    }
}
