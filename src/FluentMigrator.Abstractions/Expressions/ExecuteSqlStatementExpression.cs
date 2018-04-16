#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// Expression to execute an SQL statement
    /// </summary>
    public class ExecuteSqlStatementExpression : MigrationExpressionBase
    {
        /// <summary>
        /// Gets or sets the SQL statement to be executed
        /// </summary>
        public virtual string SqlStatement { get; set; }

        /// <inheritdoc />
        public override void ExecuteWith(IMigrationProcessor processor)
        {
            // since all the Processors are using String.Format() in their Execute method
            //  we need to escape the brackets with double brackets or else it throws an incorrect format error on the String.Format call
            var sqlText = SqlStatement.Replace("{", "{{").Replace("}", "}}");
            processor.Execute(sqlText);
        }

        /// <inheritdoc />
        public override void CollectValidationErrors(ICollection<string> errors)
        {
            if (String.IsNullOrEmpty(SqlStatement))
                errors.Add(ErrorMessages.SqlStatementCannotBeNullOrEmpty);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return base.ToString() + SqlStatement;
        }
    }
}
