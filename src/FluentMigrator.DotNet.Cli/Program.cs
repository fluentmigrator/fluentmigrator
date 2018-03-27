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
            // Register all available processor factories. The library usually tries
            // to find all provider factories by scanning all referenced assemblies,
            // but this fails if we don't have any reference. Adding the package
            // isn't enough. We MUST have a reference to a type, otherwise the
            // assembly reference gets removed by the C# compiler!
            MigrationProcessorFactoryProvider.Register(new Db2ProcessorFactory());
            MigrationProcessorFactoryProvider.Register(new DotConnectOracleProcessorFactory());
            MigrationProcessorFactoryProvider.Register(new FirebirdProcessorFactory());
            MigrationProcessorFactoryProvider.Register(new MySqlProcessorFactory());
            MigrationProcessorFactoryProvider.Register(new OracleManagedProcessorFactory());
            MigrationProcessorFactoryProvider.Register(new PostgresProcessorFactory());
            MigrationProcessorFactoryProvider.Register(new SQLiteProcessorFactory());
            MigrationProcessorFactoryProvider.Register(new SqlServer2000ProcessorFactory());
            MigrationProcessorFactoryProvider.Register(new SqlServer2005ProcessorFactory());
            MigrationProcessorFactoryProvider.Register(new SqlServer2008ProcessorFactory());
            MigrationProcessorFactoryProvider.Register(new SqlServer2012ProcessorFactory());
            MigrationProcessorFactoryProvider.Register(new SqlServer2014ProcessorFactory());
            MigrationProcessorFactoryProvider.Register(new SqlServerProcessorFactory());
            MigrationProcessorFactoryProvider.Register(new SqlServerCeProcessorFactory());

            return CommandLineApplication.Execute<Root>(args);
        }
    }
}
