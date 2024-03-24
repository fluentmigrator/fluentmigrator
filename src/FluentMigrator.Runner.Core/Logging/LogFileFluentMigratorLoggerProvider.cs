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

using System.IO;
using System.Linq;
using System.Text;

using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Logging
{
    /// <summary>
    /// Outputs the SQL statements to a log file
    /// </summary>
    public class LogFileFluentMigratorLoggerProvider : SqlScriptFluentMigratorLoggerProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogFileFluentMigratorLoggerProvider"/> class.
        /// </summary>
        /// <param name="assemblySource">The assembly source</param>
        /// <param name="options">The log file logger options</param>
        public LogFileFluentMigratorLoggerProvider(
            IAssemblySource assemblySource,
            IOptions<LogFileFluentMigratorLoggerOptions> options)
            : base(new StreamWriter(GetOutputFileName(assemblySource, options.Value), false, Encoding.UTF8), options.Value)
        {
        }

        private static string GetOutputFileName(
            IAssemblySource assemblySource,
            LogFileFluentMigratorLoggerOptions options)
        {
            if (!string.IsNullOrEmpty(options.OutputFileName))
                return options.OutputFileName;

            if (assemblySource.Assemblies.Count == 0)
                return "fluentmigrator.sql";

            var assembly = assemblySource.Assemblies.First();
            return assembly.Location + ".sql";
        }
    }
}
