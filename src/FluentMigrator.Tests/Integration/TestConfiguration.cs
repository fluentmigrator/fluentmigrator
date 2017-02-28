using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using FluentMigrator.Tests.Integration.Processors.Firebird;
using NUnit.Framework;

namespace FluentMigrator.Tests.Integration
{
    public class TestConfiguration
    {
        private const string CurrentTestConfiguration = "CurrentTestConfig";
        private string _connectionString;
        private readonly IDictionary<string, Func<string, TestProcessorFactory>> _factoryMap = new Dictionary<string, Func<string, TestProcessorFactory>>
        {
            { "Firebird", connectionString => new FirebirdTestProcessorFactory(connectionString) }
        };

        private string _testConfigFileName;

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
            return _factoryMap[RequestedDbEngine](_connectionString);
        }

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
        public string RequestedDbEngine { get; private set; }

        private void LoadConfigFile(string configFile)
        {
            var doc = XDocument.Load(configFile);
            RequestedDbEngine = doc.Root.Attribute("name").Value;
            _connectionString = doc.Root.Attribute("ConnectionString").Value;
        }
    }
}
