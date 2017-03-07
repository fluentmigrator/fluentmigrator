using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Tests.Integration
{
    public class DefaultTestFactoryFor<TProcessor> : AbstractTestProcessorFactoryOf<TProcessor>
        where TProcessor : IMigrationProcessor
    {
        private readonly string _connectionString;
        private readonly IMigrationProcessorFactory _processorFactory;

        public DefaultTestFactoryFor(string connectionString, IMigrationProcessorFactory processorFactory)
        {
            _connectionString = connectionString;
            _processorFactory = processorFactory;
        }

        public override IMigrationProcessor MakeProcessor(IAnnouncer announcer, IMigrationProcessorOptions options)
        {
            return _processorFactory.Create(_connectionString, announcer, options);
        }

        public override string ConnectionString
        {
            get { return _connectionString; }
        }
    }
}
