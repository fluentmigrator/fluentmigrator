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

using FluentMigrator.Infrastructure;
using FluentMigrator.Model;
using FluentMigrator.Validation;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to create an index
    /// </summary>
    public class CreateIndexExpression : MigrationExpressionBase, ISupportAdditionalFeatures, IIndexExpression, IValidationChildren
    {
        /// <inheritdoc />
        public virtual IndexDefinition Index { get; set; } = new IndexDefinition();

        /// <inheritdoc />
        public IDictionary<string, object> AdditionalFeatures => Index.AdditionalFeatures;

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        /// <inheritdoc />
        /// <remarks>
        /// For a non-clustered index the additional features are intentionally not carried
        /// over to the reversed expression. They describe how the index gets created (for
        /// example ONLINE or DATA_COMPRESSION on SQL Server) and are not valid options for
        /// dropping one. A clustered index keeps them, since SQL Server can drop a
        /// clustered index online and the reversal would otherwise lose that option.
        /// </remarks>
        public override IMigrationExpression Reverse()
        {
            var index = (IndexDefinition)Index.Clone();
            if (!index.IsClustered)
            {
                index.AdditionalFeatures.Clear();
            }

            return new DeleteIndexExpression { Index = index };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + Index.TableName + " (" + string.Join(", ", Index.Columns.Select(x => x.Name).ToArray()) + ")";
        }

        /// <inheritdoc />
        IEnumerable<object> IValidationChildren.Children
        {
            get { yield return Index; }
        }
    }
}
