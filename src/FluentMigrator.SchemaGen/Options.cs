using CommandLine;
using CommandLine.Text;

namespace FluentMigrator.SchemaGen
{
    public interface IOptions
    {
        string DbName { get; set; }
        string DbName1 { get; set; }
        string DbName2 { get; set; }

        string BaseDirectory { get; set; }

        string NameSpace { get; set; }

        string ClassName { get; set; }

        string MigrationVersion { get; set; }

        bool AutoReversingMigration { get; set; }

        bool UseDeprecatedTypes { get; set; }

        string IncludeTables { get; set; }

        string ExcludeTables { get; set; }
    }

    class Options : IOptions
    {
        [Option("db", Required = false, HelpText = "SQL Server database name if generating initial database code.")]
        public string DbName { get; set; }

        [Option("db1", Required = false, HelpText = "1st SQL Server database name if generating migration code.")]
        public string DbName1 { get; set; }

        [Option("db2", Required = false, HelpText = "2nd SQL Server database name if generating migration code.")]
        public string DbName2 { get; set; }

        [Option("auto-reversing", DefaultValue = true, HelpText = "Derive class from AutoReversingMigration.")]
        public bool AutoReversingMigration { get; set; }

        [Option("use-deprecated-types", DefaultValue = false, HelpText = "Use deprecated types TEXT, NTEXT and IMAGE normalled converted to VARCHAR(MAX), NVARCHAR(MAX) and VARBINARY(MAX).")]
        public bool UseDeprecatedTypes { get; set; }

        [Option("include-tables", DefaultValue = null, HelpText = "Comma separated list of table names to include. Use \"prefix*\"  to include tables with prefix.")]
        public string IncludeTables { get; set; }

        [Option("exclude-tables", DefaultValue = null, HelpText = "Comma separated list of table names to exclude. Use \"prefix*\"  to exclude tables with prefix.")]
        public string ExcludeTables { get; set; }

        [Option("dir", DefaultValue = ".", HelpText = "class directory")]
        public string BaseDirectory { get; set; }

        [Option("ns", Required = true, HelpText = "C# class namespace.")]
        public string NameSpace { get; set; }

        [Option("class", Required = false, HelpText = "C# class name.")]
        public string ClassName { get; set; }

        [Option("version", DefaultValue = "1.0", HelpText = "Database schema version.  Example: \"1.0\"")]
        public string MigrationVersion { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}