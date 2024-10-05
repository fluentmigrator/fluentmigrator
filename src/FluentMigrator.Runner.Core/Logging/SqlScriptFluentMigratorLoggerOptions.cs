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

namespace FluentMigrator.Runner.Logging
{
    /// <summary>
    /// The configuration for a <see cref="SqlScriptFluentMigratorLoggerProvider"/>
    /// </summary>
    public class SqlScriptFluentMigratorLoggerOptions : FluentMigratorLoggerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlScriptFluentMigratorLoggerOptions"/> class.
        /// </summary>
        public SqlScriptFluentMigratorLoggerOptions()
        {
            ShowSql = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether a GO statement should be output between the SQL statements
        /// </summary>
        public bool OutputGoBetweenStatements { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a semicolon (;) delimiter should be output on end the SQL statements
        /// </summary>
        public bool OutputSemicolonDelimiter { get; set; }
    }
}
