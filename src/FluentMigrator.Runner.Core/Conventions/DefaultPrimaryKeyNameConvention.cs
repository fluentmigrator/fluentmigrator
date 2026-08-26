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
            foreach (var columnDefinition in expression.Columns)
            {
                if (columnDefinition.IsPrimaryKey && string.IsNullOrEmpty(columnDefinition.PrimaryKeyName))
                {
                    var tableName = string.IsNullOrEmpty(columnDefinition.TableName)
                        ? expression.TableName
                        : columnDefinition.TableName;
                    columnDefinition.PrimaryKeyName = GetPrimaryKeyName(tableName);
                }
            }

            return expression;
        }

        private static string GetPrimaryKeyName(string tableName)
        {
            return "PK_" + tableName;
        }
    }
}
