using System;
using System.Collections.Generic;
using FluentMigrator.Runner.Announcers;
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
        }

        public void Run(Action<IMigrationProcessor> test, bool tryRollback, IEnumerable<Type> excludedProcessors)
        {
            if (_testProcessorFactory.ProcessorTypeWithin(excludedProcessors))
                Assert.Ignore("Tested feature not supported by {0}", _runningDbEngine);

            var announcer = new TextWriterAnnouncer(System.Console.Out);
            announcer.ShowSql = true;
            announcer.Heading(string.Format("Testing Migration against {0} Server", _runningDbEngine));

            using (var connection = _testProcessorFactory.MakeConnection())
            {
                var processor = _testProcessorFactory.MakeProcessor(connection, announcer);

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
