namespace FluentMigrator.Runner.Processors.Jet
{
    using System.Data.OleDb;
    using Generators.Jet;

    public class JetProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var connection = new OleDbConnection(connectionString);
            return new JetProcessor(connection, new JetGenerator(), announcer, options);
        }
    }
}