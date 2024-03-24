#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Validation;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to create a table
    /// </summary>
    public class CreateColumnExpression
        : MigrationExpressionBase,
          ISchemaExpression,
          IColumnsExpression,
          IValidationChildren
    {
        /// <inheritdoc />
        public virtual string SchemaName { get; set; }

        /// <inheritdoc />
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.TableNameCannotBeNullOrEmpty))]
        public virtual string TableName { get; set; }

        /// <summary>
        /// Gets or sets the current column definition
        /// </summary>
        public virtual ColumnDefinition Column { get; set; } = new ColumnDefinition { ModificationType = ColumnModificationType.Create };

        /// <inheritdoc />
        IEnumerable<ColumnDefinition> IColumnsExpression.Columns => new[] { Column };

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            Column.TableName = TableName;
            processor.Process(this);
        }

        /// <inheritdoc />
        public override IMigrationExpression Reverse()
        {
            return new DeleteColumnExpression
                    {
                        SchemaName = SchemaName,
                        TableName = TableName,
                        ColumnNames = {Column.Name}
                    };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var typeName = Column.Type == null ? Column.CustomType : Column.Type.ToString();
            return base.ToString() + TableName + " " + Column.Name + " " + typeName;
        }

        /// <inheritdoc />
        IEnumerable<object> IValidationChildren.Children
        {
            get { yield return Column; }
        }
    }
}
