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

using FluentMigrator.Builders.Merge;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Merge
{
    /// <summary>
    /// The implementation of the <see cref="IMergeExpressionRoot"/> interface.
    /// </summary>
    public class MergeExpressionRoot : IMergeExpressionRoot
    {
        private readonly IMigrationContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="MergeExpressionRoot"/> class.
        /// </summary>
        /// <param name="context">The migration context</param>
        public MergeExpressionRoot(IMigrationContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public IMergeDataOrInSchemaSyntax IntoTable(string tableName)
        {
            var expression = new MergeDataExpression { TableName = tableName };
            _context.Expressions.Add(expression);
            return new MergeDataExpressionBuilder(expression);
        }
    }
}