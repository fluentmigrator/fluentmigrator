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

using System;
using System.Collections.Generic;
using System.Data;

using FluentMigrator.Infrastructure;

namespace FluentMigrator.Builders.Execute
{
    /// <summary>
    /// Executes some SQL
    /// </summary>
    public interface IExecuteExpressionRoot : IFluentSyntax
    {
        /// <summary>
        /// Executes an SQL statement
        /// </summary>
        /// <param name="sqlStatement">The SQL statement to execute</param>
        void Sql(string sqlStatement);

        /// <summary>
        /// Executes an SQL statement
        /// </summary>
        /// <param name="sqlStatement">The SQL statement to execute</param>
        /// <param name="parameters">The parameters to be replaced in the SQL script</param>
        void Sql(string sqlStatement, IDictionary<string, string> parameters);

        /// <summary>
        /// Executes an SQL statement
        /// </summary>
        /// <param name="sqlStatement">The SQL statement to execute</param>
        /// <param name="description">Description for this SQL statement for logging purposes</param>
        void Sql(string sqlStatement, string description);

        /// <summary>
        /// Executes an SQL statement
        /// </summary>
        /// <param name="sqlStatement">The SQL statement to execute</param>
        /// <param name="description">Description for this SQL statement for logging purposes</param>
        /// <param name="parameters">The parameters to be replaced in the SQL script</param>
        void Sql(string sqlStatement, string description, IDictionary<string, string> parameters);

        /// <summary>
        /// Executes an SQL script loaded from the given file
        /// </summary>
        /// <param name="pathToSqlScript">The file to read the SQL script from</param>
        void Script(string pathToSqlScript);

        /// <summary>
        /// Executes an SQL script loaded from the given file
        /// </summary>
        /// <param name="pathToSqlScript">The file to read the SQL script from</param>
        /// <param name="parameters">The parameters to be replaced in the SQL script</param>
        void Script(string pathToSqlScript, IDictionary<string, string> parameters);

        /// <summary>
        /// Calls an action to execute dynamically generated SQL statements
        /// </summary>
        /// <param name="operation">The operation to execute on a given connection and transaction</param>
        void WithConnection(Action<IDbConnection, IDbTransaction> operation);

        /// <summary>
        /// Calls an action to execute dynamically generated SQL statements
        /// </summary>
        /// <param name="operation">The operation to execute on a given connection and transaction</param>
        /// <param name="description">Description for this operation for logging purposes</param>
        void WithConnection(Action<IDbConnection, IDbTransaction> operation, string description);

        /// <summary>
        /// Executes an SQL script loaded from an embedded resource
        /// </summary>
        /// <param name="embeddedSqlScriptName">The name of the embedded SQL script (partial matches allowed)</param>
        void EmbeddedScript(string embeddedSqlScriptName);

        /// <summary>
        /// Executes an SQL script loaded from an embedded resource
        /// </summary>
        /// <param name="embeddedSqlScriptName">The name of the embedded SQL script (partial matches allowed)</param>
        /// <param name="parameters">The parameters to be replaced in the SQL script</param>
        void EmbeddedScript(string embeddedSqlScriptName, IDictionary<string, string> parameters);
    }
}
