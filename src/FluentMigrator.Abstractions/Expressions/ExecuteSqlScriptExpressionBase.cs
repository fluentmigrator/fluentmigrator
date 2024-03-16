#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using System.Collections.Generic;

namespace FluentMigrator.Expressions
{
    /// <summary>
    /// The base class for SQL script execution
    /// </summary>
    public abstract class ExecuteSqlScriptExpressionBase : MigrationExpressionBase
    {
        /// <summary>
        /// Gets or sets parameters to be replaced before script execution
        /// </summary>
        public IDictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Executes the <paramref name="sqlScript"/> with the given <paramref name="processor"/>
        /// </summary>
        /// <param name="processor">The processor to execute the script with</param>
        /// <param name="sqlScript">The SQL script to execute</param>
        protected void Execute(IMigrationProcessor processor, string sqlScript)
        {
            var finalSqlScript = SqlScriptTokenReplacer.ReplaceSqlScriptTokens(sqlScript, Parameters);
            processor.Execute(finalSqlScript);
        }
    }
}
