#region Apache License
// 
// Copyright (c) 2014, Tony O'Hagan <tony@ohagan.name>
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

using CommandLine;
using CommandLine.Text;

namespace FluentMigrator.SchemaGen
{
    public interface IOptions
    {
        /// <summary>
        /// Connection Strings 
        /// </summary>
        string Db { get; }
        string Db1 { get; }
        string Db2 { get; }

        string OutputDirectory { get; }
        string NameSpace { get; }
        string MigrationVersion { get; }
        int StepStart { get; }
        int StepEnd { get; }
        string Tags { get; }

        string IncludeTables { get; }
        string ExcludeTables { get; }

        string SqlDirectory { get; }

        bool UseDeprecatedTypes { get; }
        bool ShowOldCode { get; }
        bool DropScripts { get; }
        bool DropTables { get; }
    }

    /// <summary>
    /// Used by CommandLineParser from NuGet
    /// </summary>
    internal class Options : IOptions
    {
        [Option("db", Required = false, HelpText = "SQL Server database name (or connection string) for generating full database schema.")]
        public string Db { get; set; }

        [Option("db1", Required = false, HelpText = "1st SQL Server database name (or connection string) if generating migration difference.")]
        public string Db1 { get; set; }

        [Option("db2", Required = false, HelpText = "2nd SQL Server database name if generating migration code.")]
        public string Db2 { get; set; }

        
        [Option("dir", DefaultValue = ".", HelpText = "class directory")]
        public string OutputDirectory { get; set; }

        [Option("ns", Required = true, HelpText = "C# class namespace.")]
        public string NameSpace { get; set; }

        [Option("version", DefaultValue = "1.0.0", HelpText = "Database schema version.  Example: \"3.1.1\"")]
        public string MigrationVersion { get; set; }

        [Option("step-start", DefaultValue = 1, HelpText = "First step number. Appended to Version number")]
        public int StepStart { get; set; }

        [Option("step-end", DefaultValue = -1, HelpText = "Last step number. Adds a final Migration class just to set the step value.")]
        public int StepEnd { get; set; }

        [Option("tags", DefaultValue = "", HelpText = "Example: --tags abc,def Adds [Tags(\"tag1\", \"def\")] attribute to all generated C# classes.")]
        public string Tags { get; set; }

        [Option("use-deprecated-types", DefaultValue = false, HelpText = "Use deprecated types TEXT, NTEXT and IMAGE normalled converted to VARCHAR(MAX), NVARCHAR(MAX) and VARBINARY(MAX).")]
        public bool UseDeprecatedTypes { get; set; }

        [Option("include-tables", DefaultValue = null, HelpText = "Comma separated list of table names to include. Use \"prefix*\"  to include tables with prefix.")]
        public string IncludeTables { get; set; }

        [Option("exclude-tables", DefaultValue = null, HelpText = "Comma separated list of table names to exclude. Use \"prefix*\"  to exclude tables with prefix.")]
        public string ExcludeTables { get; set; }

        [Option("show-old-schema", DefaultValue = false, HelpText = "Shows old schema code from Db1 as comments. Very useful to understand the changes.")]
        public bool ShowOldCode { get; set; }

        [Option("drop-scripts", DefaultValue = false, HelpText = "Generates a class to drop user defined types, functions, stored procedures and views in Db1 but removed from Db2.")]
        public bool DropScripts { get; set; }

        [Option("drop-tables", DefaultValue = false, HelpText = "Generates a class to drop tables that were in Db1 but removed from Db2.")]
        public bool DropTables { get; set; }

        [Option("sql-dir", DefaultValue = null, HelpText = "If set, Adds Sql.Execute() statements from SQL script files. Turn on to discover the expected subfolders.")]
        public string SqlDirectory { get; set; }


        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}