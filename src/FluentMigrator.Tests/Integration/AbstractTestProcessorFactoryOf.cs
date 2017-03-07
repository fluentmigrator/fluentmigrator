using System;
using FluentMigrator.Runner;

namespace FluentMigrator.Tests.Integration
{
    public abstract class AbstractTestProcessorFactoryOf<TProcessor> : TestProcessorFactory
        where TProcessor : IMigrationProcessor
    {
        public abstract IMigrationProcessor MakeProcessor(IAnnouncer announcer, IMigrationProcessorOptions options);

        public Type GetProcessorType()
        {
            return typeof(TProcessor);
        }

        public abstract string ConnectionString { get; }
    }
}
