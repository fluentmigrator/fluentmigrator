using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator.Runner;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    public abstract class AbstractTestProcessorFactoryOf<TProcessor> : TestProcessorFactory
        where TProcessor : IMigrationProcessor
    {
        public abstract void Done();

        public abstract IDbConnection MakeConnection();

        public abstract IMigrationProcessor MakeProcessor(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options);

        public bool ProcessorTypeWithin(IEnumerable<Type> candidates)
        {
            return candidates.Any(t => typeof(TProcessor).IsAssignableFrom(t));
        }
    }
}
