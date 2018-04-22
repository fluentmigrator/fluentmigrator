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
            var command = connection.CreateCommand();
            command.CommandText = commandText;
            // SQL Server CE does not support non-zero command timeout values!! :/
            if (transaction != null) command.Transaction = transaction;
            return command;
        }
    }
}
