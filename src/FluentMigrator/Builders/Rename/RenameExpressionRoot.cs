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

using FluentMigrator.Builders.Rename.Column;
using FluentMigrator.Builders.Rename.Table;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Rename
{
    public class RenameExpressionRoot : IRenameExpressionRoot
    {
        private readonly IMigrationContext _context;

        public RenameExpressionRoot(IMigrationContext context)
        {
            _context = context;
        }

        public IRenameTableToOrInSchemaSyntax Table(string oldName)
        {
            var expression = new RenameTableExpression { OldName = oldName };
            _context.Expressions.Add(expression);
            return new RenameTableExpressionBuilder(expression);
        }

        public IRenameColumnTableSyntax Column(string oldName)
        {
            var expression = new RenameColumnExpression { OldName = oldName };
            _context.Expressions.Add(expression);
            return new RenameColumnExpressionBuilder(expression);
        }
    }
}