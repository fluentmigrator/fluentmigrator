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

using System.Text;

using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Conventions
{
    /// <summary>
    /// The default implementation of a <see cref="IIndexConvention"/>
    /// </summary>
    /// <remarks>
    /// It sets the default name of an index.
    /// </remarks>
    public class DefaultIndexNameConvention : IIndexConvention
    {
        /// <inheritdoc />
        public IIndexExpression Apply(IIndexExpression expression)
        {
            if (string.IsNullOrEmpty(expression.Index.Name))
            {
                expression.Index.Name = GetIndexName(expression.Index);
            }

            return expression;
        }

        private static string GetIndexName(IndexDefinition index)
        {
            var sb = new StringBuilder();

            sb.Append("IX_");
            sb.Append(index.TableName);

            foreach (IndexColumnDefinition column in index.Columns)
            {
                sb.Append("_");
                sb.Append(column.Name);
            }

            return sb.ToString();
        }
    }
}
