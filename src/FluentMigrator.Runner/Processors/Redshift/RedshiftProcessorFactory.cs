namespace FluentMigrator.Runner.Processors.Redshift
{
    using Generators.Redshift;

    public class RedshiftProcessorFactory : MigrationProcessorFactory
    {
        public override IMigrationProcessor Create(string connectionString, IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            var factory = new RedshiftDbFactory();
            var connection = factory.CreateConnection(connectionString);
            return new RedshiftProcessor(connection, new RedshiftGenerator(), announcer, options, factory);
        }
    }
}