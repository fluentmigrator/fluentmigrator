using System;
using System.Configuration;
using System.IO;
using System.Linq;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Initialization
{
	public class RunnerContext : IRunnerContext
	{
		public RunnerContext(IAnnouncer announcer)
		{
			Announcer = announcer;
			StopWatch = new StopWatch();
		}

		private string ConfigFile;
		private string ConnectionString;
		private IMigrationProcessor _processor;

		private bool NotUsingConfig
		{
			get { return string.IsNullOrEmpty(ConfigFile); }
		}

		public string Database { get; set; }
		public string Connection { get; set; }
		public string Target { get; set; }
		public bool PreviewOnly { get; set; }
		public string Namespace { get; set; }
		public string Task { get; set; }
		public long Version { get; set; }
		public int Steps { get; set; }
		public string WorkingDirectory { get; set; }
		public string Profile { get; set; }
		public int Timeout { get; set; }
		public string DotNetVersion { get; set; }

		public IAnnouncer Announcer
		{
			get; set;
		}

		/// <summary>
		/// Uses a connection string named in the connectionStrings portion of the machine.config or app.config
		/// </summary>
		public void UseConnectionName(string connectionStringName)
		{
			if (!string.IsNullOrEmpty(connectionStringName))
			{
				var manager = new NetConfigManager(DotNetVersion);
				Connection = manager.GetConnectionString(connectionStringName);
			}
		}

		public IStopWatch StopWatch
		{
			get; private set;
		}

		public IMigrationProcessor Processor
		{
			get
			{
				if (_processor != null)
				{
					return _processor;
				}

				var configFile = Path.Combine(Environment.CurrentDirectory, Target);
				if (File.Exists(configFile + ".config"))
				{
					var config = ConfigurationManager.OpenExeConfiguration(configFile);
					var connections = config.ConnectionStrings.ConnectionStrings;

					if (connections.Count > 1)
					{
						if (string.IsNullOrEmpty(Connection))
						{
							ReadConnectionString(connections[Environment.MachineName], config.FilePath);
						}
						else
						{
							ReadConnectionString(connections[Connection], config.FilePath);
						}
					}
					else if (connections.Count == 1)
					{
						ReadConnectionString(connections[0], config.FilePath);
					}
				}

				if (NotUsingConfig && !string.IsNullOrEmpty(Connection))
				{
					ConnectionString = Connection;
				}

				if (string.IsNullOrEmpty(ConnectionString))
				{
					throw new ArgumentException("Connection String or Name is required \"/connection\"");
				}

				if (string.IsNullOrEmpty(Database))
				{
					throw new ArgumentException(
						"Database Type is required \"/db [db type]\". Available db types is [sqlserver], [sqlite]");
				}

				if (NotUsingConfig)
				{
					Console.WriteLine("Using Database {0} and Connection String {1}", Database, ConnectionString);
				}
				else
				{
					Console.WriteLine("Using Connection {0} from Configuration file {1}", Connection, ConfigFile);
				}

				if (Timeout == 0)
				{
					Timeout = 30; // Set default timeout for command
				}

				var processorFactory = ProcessorFactory.GetFactory(Database);
				_processor = processorFactory.Create(ConnectionString, Announcer, new ProcessorOptions
																					{
																						PreviewOnly = PreviewOnly,
																						Timeout = Timeout
																					});

				return _processor;
			}
		}

		private void ReadConnectionString(ConnectionStringSettings connection, string configurationFile)
		{
			if (connection != null)
			{
				var factory = ProcessorFactory.Factories.Where(f => f.IsForProvider(connection.ProviderName)).FirstOrDefault();
				if (factory != null)
				{
					Database = factory.Name;
					Connection = connection.Name;
					ConnectionString = connection.ConnectionString;
					ConfigFile = configurationFile;
				}
			}
			else
			{
				Console.WriteLine("connection is null!");
			}
		}
	}
}