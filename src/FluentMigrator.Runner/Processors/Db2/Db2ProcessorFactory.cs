namespace FluentMigrator.Runner.Processors.DB2
{
    using FluentMigrator.Runner.Generators.DB2;

    public class Db2ProcessorFactory : MigrationProcessorFactory
    {
        #region Methods

        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            System.Func<IDbFactory> factory = () => new Db2DbFactory();
            System.Func<System.Data.IDbConnection> connection = () => factory().CreateConnection(connectionString);
            return new Db2Processor(connection, new Db2Generator(), announcer, options, factory);
        }

        #endregion Methods
    }
}