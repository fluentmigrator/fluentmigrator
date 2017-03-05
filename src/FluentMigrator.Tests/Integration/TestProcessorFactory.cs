using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Runner;

namespace FluentMigrator.Tests.Integration
{
    public interface TestProcessorFactory
    {
        IMigrationProcessor MakeProcessor(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options);
        IDbConnection MakeConnection();
        Type GetProcessorType();
    }
}
