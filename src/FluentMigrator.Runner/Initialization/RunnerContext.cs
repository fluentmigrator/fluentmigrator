using System;
using System.Configuration;
using System.IO;
using System.Linq;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Initialization {
	public class RunnerContext : IRunnerContext {
		private string ConnectionString;
		private bool NotUsingConfig = true;
		private IMigrationProcessor _processor;

		#region IRunnerContext Members

		public string Database { get; set; }
		public string Connection { get; set; }
		public string Target { get; set; }
		public bool LoggingEnabled { get; set; }
		public string Namespace { get; set; }
		public string Task { get; set; }
		public long Version { get; set; }
		public int Steps { get; set; }
		public string WorkingDirectory { get; set; }

		public IMigrationProcessor Processor {
			get {
				if(_processor != null) {
					return _processor;
				}

				string configFile = Path.Combine(Environment.CurrentDirectory, Target) + ".config";
				if(File.Exists(configFile)) {
					Configuration config = ConfigurationManager.OpenExeConfiguration(configFile);
					ConnectionStringSettingsCollection connections = config.ConnectionStrings.ConnectionStrings;
					if(connections.Count > 1) {
						ReadConnectionString(connections[Connection]);
					}
					else if(connections.Count == 1) {
						ReadConnectionString(connections[0]);
					}
				}

				if(NotUsingConfig && !string.IsNullOrEmpty(Connection)) {
					ConnectionString = Connection;
				}

				if(string.IsNullOrEmpty(Database)) {
					throw new ArgumentException(
						"Database Type is required \"/db [db type]\". Available db types is [sqlserver], [sqlite]");
				}

				if(string.IsNullOrEmpty(ConnectionString)) {
					throw new ArgumentException("Connection String or Name is required \"/connection\"");
				}

				IMigrationProcessorFactory processorFactory = ProcessorFactory.GetFactory(Database);
				_processor = processorFactory.Create(ConnectionString);

				return _processor;
			}
		}

		#endregion

		private void ReadConnectionString(ConnectionStringSettings connection) {
			if(connection != null) {
				IMigrationProcessorFactory factory =
					ProcessorFactory.Factories.Where(f => f.IsForProvider(connection.ProviderName)).FirstOrDefault();
				if(factory != null) {
					Database = factory.Name;
					ConnectionString = connection.ConnectionString;
					NotUsingConfig = false;
				}
			}
		}
	}
}