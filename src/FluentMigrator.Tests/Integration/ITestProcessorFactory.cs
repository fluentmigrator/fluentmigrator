using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Runner;

namespace FluentMigrator.Tests.Integration
{
    public interface TestProcessorFactory
    {
        IMigrationProcessor MakeProcessor(IDbConnection connection, IAnnouncer announcer);
        IDbConnection MakeConnection();
        bool ProcessorTypeWithin(IEnumerable<Type> excludedProcessors);
        void Done();
    }
}
