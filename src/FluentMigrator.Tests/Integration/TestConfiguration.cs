using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Tests.Integration.Processors.Firebird;
using FluentMigrator.Tests.Integration.Processors.SQLite;
using FluentMigrator.Tests.Integration.Processors.SqlServer;
using NUnit.Framework;

namespace FluentMigrator.Tests.Integration
{
    public class TestConfiguration
    {
        private const string CurrentTestConfiguration = "CurrentTestConfig";
        private string _connectionString;
        private string _testConfigFileName;
        private readonly IDictionary<string, Func<string, TestProcessorFactory>> _factoryMap = new Dictionary<string, Func<string, TestProcessorFactory>>
        {
            { "Firebird", connectionString => new FirebirdTestProcessorFactory(connectionString) },
            { "SqlServer2012", connectionString => new SqlServerTestProcessorFactory(connectionString, new SqlServer2012Generator()) },
            { "SQLite", connectionString => new SQLiteTestProcessorFactory(connectionString) }
        };

        public TestConfiguration(string testConfigFileName)
        {
            _testConfigFileName = testConfigFileName;
        }

        public void Configure()
        {
            var configFile = FindConfigFile();
            if (configFile == null)
                Assert.Fail("Test configuration was not found. Please ensure that the file '{0}' ist present under a folder named '{1}' in the root directory of the repo.", _testConfigFileName, CurrentTestConfiguration);

            LoadConfigFile(configFile);
        }

        public TestProcessorFactory GetProcessorFactory()
        {
            Func<string, TestProcessorFactory> result = null;
            if (!_factoryMap.TryGetValue(RequestedDbEngine, out result))
                Assert.Fail("A TestProcessorFactory for {0} was not found. Please add a proper implementation of TestProcessorFactory to the class TestConfiguration", RequestedDbEngine);

            return result(_connectionString);
        }

        public string RequestedDbEngine { get; private set; }

        private string FindConfigFile()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var relativeSearchPath = AppDomain.CurrentDomain.RelativeSearchPath;
            var folder = relativeSearchPath == null ? baseDir : Path.Combine(baseDir, relativeSearchPath);

            while (folder != null)
            {
                string current = Path.Combine(Path.Combine(folder, CurrentTestConfiguration), _testConfigFileName);
                if (File.Exists(current))
                    return current;
                folder = Path.GetDirectoryName(folder);
            }

            return null;
        }

        private void LoadConfigFile(string configFile)
        {
            var doc = XDocument.Load(configFile);
            RequestedDbEngine = doc.Root.Attribute("name").Value;
            _connectionString = doc.Root.Attribute("ConnectionString").Value;
        }
    }
}
