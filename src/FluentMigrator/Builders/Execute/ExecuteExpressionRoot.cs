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

using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace FluentMigrator.Builders.Execute
{
    public class ExecuteExpressionRoot : IExecuteExpressionRoot
    {
        private readonly IMigrationContext _context;

        public ExecuteExpressionRoot(IMigrationContext context)
        {
            _context = context;
        }

        public void Sql(string sqlStatement)
        {
            var expression = new ExecuteSqlStatementExpression { SqlStatement = sqlStatement };
            _context.Expressions.Add(expression);
        }

        public void Script(string pathToSqlScript)
        {
            var expression = new ExecuteSqlScriptExpression { SqlScript = pathToSqlScript };
            _context.Expressions.Add(expression);
        }

        public void WithConnection(Action<IDbConnection, IDbTransaction> operation)
        {
            var expression = new PerformDBOperationExpression { Operation = operation };
            _context.Expressions.Add(expression);
        }

        public void EmbeddedScript(string EmbeddedSqlScriptName)
        {
            var expression = new ExecuteEmbeddedSqlScriptExpression { SqlScript = EmbeddedSqlScriptName, MigrationAssemblies = _context.MigrationAssemblies };
            _context.Expressions.Add(expression);
        }
    }

    public class PerformDBOperationExpression : MigrationExpressionBase
    {
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            processor.Process(this);
        }

        public override void CollectValidationErrors(ICollection<string> errors)
        {
            if (Operation == null)
                errors.Add(ErrorMessages.OperationCannotBeNull);
        }

        public Action<IDbConnection, IDbTransaction> Operation { get; set; }
    }
}
