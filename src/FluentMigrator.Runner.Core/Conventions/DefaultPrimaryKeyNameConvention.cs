#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Linq;

using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Conventions
{
    /// <summary>
    /// The default implementation of a <see cref="IColumnsConvention"/>
    /// </summary>
    /// <remarks>
    /// It sets the default constraint name of a primary key column.
    /// </remarks>
    public class DefaultPrimaryKeyNameConvention : IColumnsConvention
    {
        /// <inheritdoc />
        public IColumnsExpression Apply(IColumnsExpression expression)
        {
            if (expression.Columns.Count(x => x.IsPrimaryKey) > 1)
                throw new InvalidOperationException("Error creating table with multiple primary keys.");

            foreach (var columnDefinition in expression.Columns)
            {
                if (string.IsNullOrEmpty(columnDefinition.Name))
                    throw new ArgumentException("An object or column name is missing or empty.");

                if (columnDefinition.IsPrimaryKey && string.IsNullOrEmpty(columnDefinition.PrimaryKeyName))
                {
                    var tableName = string.IsNullOrEmpty(columnDefinition.TableName)
                        ? expression.TableName
                        : columnDefinition.TableName;
                    columnDefinition.PrimaryKeyName = $"PK_{tableName}_{columnDefinition.Name}";
                }
            }

            return expression;
        }
    }
}
