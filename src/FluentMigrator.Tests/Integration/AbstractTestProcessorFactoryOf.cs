﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator.Runner;

namespace FluentMigrator.Tests.Integration
{
    public abstract class AbstractTestProcessorFactoryOf<TProcessor> : TestProcessorFactory
        where TProcessor : IMigrationProcessor
    {
        public abstract IDbConnection MakeConnection();

        public abstract IMigrationProcessor MakeProcessor(IDbConnection connection, IAnnouncer announcer, IMigrationProcessorOptions options);

        public Type GetProcessorType()
        {
            return typeof(TProcessor);
        }
    }
}