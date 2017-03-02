using System;
using System.Collections.Generic;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using NUnit.Framework;

namespace FluentMigrator.Tests.Integration
{
    public class TestDriver
    {
        private readonly TestProcessorFactory _testProcessorFactory;
        private readonly string _runningDbEngine;

        public TestDriver(TestProcessorFactory testProcessorFactory, string runningDbEngine)
        {
            _testProcessorFactory = testProcessorFactory;
            _runningDbEngine = runningDbEngine;
        }

        public void Run(Action<IMigrationProcessor> test, IAnnouncer announcer, IMigrationProcessorOptions options, bool tryRollback, IEnumerable<Type> excludedProcessors)
        {
            if (_testProcessorFactory.ProcessorTypeWithin(excludedProcessors))
                Assert.Ignore("Tested feature not supported by {0}", _runningDbEngine);

            using (var connection = _testProcessorFactory.MakeConnection())
            {
                if (options == null)
                    options = new ProcessorOptions();
                var processor = _testProcessorFactory.MakeProcessor(connection, announcer, options);

                test(processor);

                var baseProcessor = processor as ProcessorBase;
                if (tryRollback && baseProcessor != null && !baseProcessor.WasCommitted)
                {
                    processor.RollbackTransaction();
                }
            }

            _testProcessorFactory.Done();
        }
    }
}
