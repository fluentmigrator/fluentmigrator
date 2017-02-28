using System;
using System.IO;
using NUnit.Framework;

namespace FluentMigrator.Tests.Integration
{
	public class TestConfiguration
	{
		private const string CurrentTestConfiguration = "CurrentTestConfig";
		private string _testConfigFileName;

		public TestConfiguration(string testConfigFileName)
		{
			_testConfigFileName = testConfigFileName;
		}

		public void Configure()
		{
			var configFile = FindConfigFile();
			if (configFile == null)
				Assert.Fail("Test configuration was not found. Please ensure that the file 'TestConfig.xml' ist present under a folder named 'CurrentTestConfig' in the root directory of the repo.");
		}

		public object GetProcessor()
		{
			return null;
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
	}
}
