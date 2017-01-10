namespace FluentMigrator.Runner.Processors
{
    using System.Data;

    public interface IDbFactory
    {
        IDbConnection CreateConnection(string connectionString);
        IDbCommand CreateCommand(string commandText, IDbConnection connection, IDbTransaction transaction);
#if !COREFX
        IDbDataAdapter CreateDataAdapter(IDbCommand command);
#endif
        IDbCommand CreateCommand(string commandText, IDbConnection connection);
    }
}