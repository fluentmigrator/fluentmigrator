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

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Update
{
    /// <summary>
    /// The implementation of the <see cref="IUpdateExpressionRoot"/> interface.
    /// </summary>
    public class UpdateExpressionRoot : IUpdateExpressionRoot
    {
        private readonly IMigrationContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateExpressionRoot"/> class.
        /// </summary>
        /// <param name="context">The migration context</param>
        public UpdateExpressionRoot(IMigrationContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public IMigrationContext GetMigrationContext() => _context;

        /// <inheritdoc />
        public IUpdateSetOrInSchemaSyntax Table(string tableName)
        {
            var expression = new UpdateDataExpression { TableName = tableName };
            _context.Expressions.Add(expression);
            return new UpdateDataExpressionBuilder(expression);
        }
    }
}
