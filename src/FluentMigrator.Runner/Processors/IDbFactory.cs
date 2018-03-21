using System.Data.Common;

namespace FluentMigrator.Runner.Processors
{
    public interface IDbFactory
    {
        DbConnection CreateConnection(string connectionString);
        DbCommand CreateCommand(string commandText, DbConnection connection, DbTransaction transaction);
        DbDataAdapter CreateDataAdapter(DbCommand command);
        DbCommand CreateCommand(string commandText, DbConnection connection);
    }
}