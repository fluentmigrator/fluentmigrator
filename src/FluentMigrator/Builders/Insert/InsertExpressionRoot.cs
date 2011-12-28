#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

namespace FluentMigrator.Builders.Insert
{
    public class InsertExpressionRoot : IInsertExpressionRoot
    {
        private readonly IMigrationContext _context;

        public InsertExpressionRoot(IMigrationContext context)
        {
            _context = context;
        }

        public IInsertDataOrInSchemaSyntax IntoTable(string tableName)
        {
            var expression = new InsertDataExpression { TableName = tableName };
            _context.Expressions.Add(expression);
            return new InsertDataExpressionBuilder(expression);
        }
    }
}