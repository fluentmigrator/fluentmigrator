namespace FluentMigrator.Runner.Processors
{
    using System.Data;

    public interface IDbFactory
    {
        IDbConnection CreateConnection(string connectionString);
        IDbCommand CreateCommand(string commandText, IDbConnection connection, IDbTransaction transaction);
        IDbDataAdapter CreateDataAdapter(IDbCommand command);
        IDbCommand CreateCommand(string commandText, IDbConnection connection);
    }
}