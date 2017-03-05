using System;
using System.Collections.Generic;
using System.Linq;
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
            var processorType = _testProcessorFactory.GetProcessorType();
            if (excludedProcessors.Any(t => processorType.IsAssignableFrom(t)))
                Assert.Ignore("Tested feature not supported by {0} or test is intended for another db engine", _runningDbEngine);

            if (options == null)
                options = new ProcessorOptions();
            using (var processor = _testProcessorFactory.MakeProcessor(announcer, options))
            {
                test(processor);

                var baseProcessor = processor as ProcessorBase;
                if (tryRollback && baseProcessor != null && !baseProcessor.WasCommitted)
                {
                    processor.RollbackTransaction();
                }
            }
        }
    }
}
