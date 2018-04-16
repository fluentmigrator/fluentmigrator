#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;

using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;

namespace FluentMigrator.Model
{
    /// <summary>
    /// The constraint definition
    /// </summary>
    public class ConstraintDefinition : ICloneable, ICanBeValidated, ISupportAdditionalFeatures
    {
        private ConstraintType constraintType;

        /// <summary>
        /// Gets a value indicating whether the constraint is a primary key constraint
        /// </summary>
        public bool IsPrimaryKeyConstraint { get { return ConstraintType.PrimaryKey == constraintType; } }

        /// <summary>
        /// Gets a value indicating whether the constraint is a unique constraint
        /// </summary>
        public bool IsUniqueConstraint { get { return ConstraintType.Unique == constraintType; } }

        /// <summary>
        /// Gets or sets the schema name
        /// </summary>
        public virtual string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the constraint name
        /// </summary>
        public virtual string ConstraintName { get; set; }

        /// <summary>
        /// Gets or sets the table name
        /// </summary>
        public virtual string TableName { get; set; }

        /// <summary>
        /// Gets or sets the column names
        /// </summary>
        public virtual ICollection<string> Columns { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ConstraintDefinition"/> class.
        /// </summary>
        public ConstraintDefinition(ConstraintType type)
        {
            constraintType = type;

            Columns = new HashSet<string>();
        }

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalFeatures { get; } = new Dictionary<string, object>();

        /// <inheritdoc />
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

        /// <inheritdoc />
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
    }
}
