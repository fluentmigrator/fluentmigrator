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
using System.Linq;

using FluentMigrator.Model;
using FluentMigrator.Validation;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to create a foreign key
    /// </summary>
    public class CreateForeignKeyExpression : MigrationExpressionBase, IForeignKeyExpression, IValidationChildren
    {
        /// <inheritdoc />
        public virtual ForeignKeyDefinition ForeignKey { get; set; } = new ForeignKeyDefinition();

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        /// <inheritdoc />
        public override IMigrationExpression Reverse()
        {
            return new DeleteForeignKeyExpression { ForeignKey = ForeignKey.Clone() as ForeignKeyDefinition };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() +
                    string.Format("{0} {1}({2}) {3}({4})",
                                ForeignKey.Name,
                                ForeignKey.ForeignTable,
                                string.Join(", ", ForeignKey.ForeignColumns.ToArray()),
                                ForeignKey.PrimaryTable,
                                string.Join(", ", ForeignKey.PrimaryColumns.ToArray()));
        }

        /// <inheritdoc />
        IEnumerable<object> IValidationChildren.Children
        {
            get { yield return ForeignKey; }
        }
    }
}
