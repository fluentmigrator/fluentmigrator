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

using System.ComponentModel.DataAnnotations;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to alter the table description
    /// </summary>
    public class AlterTableExpression : MigrationExpressionBase,
        ISchemaExpression
    {
        /// <inheritdoc />
        public virtual string SchemaName { get; set; }

        /// <summary>
        /// Gets or sets the table name
        /// </summary>
        [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.TableNameCannotBeNullOrEmpty))]
        public virtual string TableName { get; set; }

        /// <summary>
        /// Gets or sets the table description
        /// </summary>
        public virtual string TableDescription { get; set; }

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        /// <inheritdoc />
        public override IMigrationExpression Reverse()
        {
            return new AlterTableExpression
            {
                SchemaName = SchemaName,
                TableName = TableName,
                TableDescription = TableDescription
            };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + TableName;
        }
    }
}
