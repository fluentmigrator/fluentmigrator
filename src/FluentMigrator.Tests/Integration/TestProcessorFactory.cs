using System;
using FluentMigrator.Runner;

namespace FluentMigrator.Tests.Integration
{
    public interface TestProcessorFactory
    {
        IMigrationProcessor MakeProcessor(IAnnouncer announcer, IMigrationProcessorOptions options);
        Type GetProcessorType();
    }
}
