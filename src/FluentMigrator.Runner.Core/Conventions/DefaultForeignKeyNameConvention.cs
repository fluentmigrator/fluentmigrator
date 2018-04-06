#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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
    /// The default implementation of a <see cref="IForeignKeyConvention"/>
    /// </summary>
    /// <remarks>
    /// It sets the default name of a foreign key.
    /// </remarks>
    public class DefaultForeignKeyNameConvention : IForeignKeyConvention
    {
        /// <inheritdoc />
        public IForeignKeyExpression Apply(IForeignKeyExpression expression)
        {
            if (string.IsNullOrEmpty(expression.ForeignKey.Name))
            {
                expression.ForeignKey.Name = GetForeignKeyName(expression.ForeignKey);
            }

            return expression;
        }

        private static string GetForeignKeyName(ForeignKeyDefinition foreignKey)
        {
            var sb = new StringBuilder();

            sb.Append("FK_");
            sb.Append(foreignKey.ForeignTable);

            foreach (string foreignColumn in foreignKey.ForeignColumns)
            {
                sb.Append("_");
                sb.Append(foreignColumn);
            }

            sb.Append("_");
            sb.Append(foreignKey.PrimaryTable);

            foreach (string primaryColumn in foreignKey.PrimaryColumns)
            {
                sb.Append("_");
                sb.Append(primaryColumn);
            }

            return sb.ToString();
        }
    }
}
