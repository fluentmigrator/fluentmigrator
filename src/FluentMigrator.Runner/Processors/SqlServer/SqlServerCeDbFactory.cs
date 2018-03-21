using System.Data;

namespace FluentMigrator.Runner.Processors.SqlServer
{
    public class SqlServerCeDbFactory : ReflectionBasedDbFactory
    {
        public SqlServerCeDbFactory()
            : base("System.Data.SqlServerCe", "System.Data.SqlServerCe.SqlCeProviderFactory")
        {
        }

        public override IDbCommand CreateCommand(string commandText, IDbConnection connection, IDbTransaction transaction, IMigrationProcessorOptions options)
        {
            var command = base.CreateCommand(commandText, connection, transaction, options);

            if (command.CommandTimeout != 0)
            {
                command.CommandTimeout = 0; // SQL Server CE does not support non-zero command timeout values!! :/
            }

            return command;
        }
    }
}