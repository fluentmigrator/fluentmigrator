using FluentMigrator.DotNet.Cli.Commands;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.DB2;
using FluentMigrator.Runner.Processors.DotConnectOracle;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Runner.Processors.Postgres;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.Runner.Processors.SQLite;

using McMaster.Extensions.CommandLineUtils;

namespace FluentMigrator.DotNet.Cli
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return CommandLineApplication.Execute<Root>(args);
        }
    }
}
