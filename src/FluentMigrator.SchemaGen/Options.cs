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
        string Db { get; set; }
        string Db1 { get; set; }
        string Db2 { get; set; }

        string BaseDirectory { get; set; }
        string NameSpace { get; set; }
        string MigrationVersion { get; set; }
        int StepStart { get; set; }
        int StepEnd { get; set; }
        string Tags { get; set; }

        bool UseDeprecatedTypes { get; set; }
        string IncludeTables { get; set; }
        string ExcludeTables { get; set; }
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
        public string BaseDirectory { get; set; }

        [Option("ns", Required = true, HelpText = "C# class namespace.")]
        public string NameSpace { get; set; }

        [Option("version", DefaultValue = "1.0", HelpText = "Database schema version.  Example: \"1.0\"")]
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
        
        
        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}