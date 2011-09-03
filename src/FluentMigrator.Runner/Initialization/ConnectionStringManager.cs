using System;
using System.Configuration;
using System.IO;
using System.Linq;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Locates connection strings by name in assembly's config file or machine.config
    /// If no connection matches it uses the specified connection string as valid connection
    /// </summary>
    public class ConnectionStringManager
    {
        private readonly string configPath;
        private readonly string assemblyLocation;
        private string connection;
        private string database;
        private string configFile;
        private bool notUsingConfig;
        private INetConfigManager configManager;

        public ConnectionStringManager(INetConfigManager configManager, string connection, string configPath, string assemblyLocation, string database)
        {
            this.connection = connection;
            this.configPath = configPath;
            this.database = database;
            this.assemblyLocation = assemblyLocation;
            this.notUsingConfig = true;
            this.configManager = configManager;
        }

        public void LoadConnectionString()
        {
            if (!String.IsNullOrEmpty(connection))
            {
                if (notUsingConfig && !String.IsNullOrEmpty(configFile))
                    LoadConnectionStringFromConfigurationFile(configManager.LoadFromFile(configFile), false);

                if (notUsingConfig && !String.IsNullOrEmpty(assemblyLocation))
                {
                    string defaultConfigFile = assemblyLocation;

                    LoadConnectionStringFromConfigurationFile(configManager.LoadFromFile(defaultConfigFile), false);
                }

                if (notUsingConfig)
                    LoadConnectionStringFromConfigurationFile(configManager.LoadFromMachineConfiguration(), false);

                if (notUsingConfig)
                {
                    if (notUsingConfig && !string.IsNullOrEmpty(connection))
                    {
                        ConnectionString = connection;
                    }
                }
            }
            else
                LoadConnectionStringFromConfigurationFile(configManager.LoadFromMachineConfiguration(), true);

            OutputResults();
        }

        private void LoadConnectionStringFromConfigurationFile(Configuration configurationFile, bool useDefault)
        {
            var connections = configurationFile.ConnectionStrings.ConnectionStrings;

            if (connections == null || connections.Count <= 0)
                return;

            ConnectionStringSettings connectionString;

            if (useDefault)
                connectionString = connections[0];
            else if (string.IsNullOrEmpty(connection))
                connectionString = connections[Environment.MachineName];
            else
                connectionString = connections[connection];

            ReadConnectionString(connectionString, configurationFile.FilePath);
        }

        private void ReadConnectionString(ConnectionStringSettings connectionSetting, string configurationFile)
        {
            if (connectionSetting != null)
            {
                var factory = ProcessorFactory.Factories.Where(f => f.IsForProvider(database)).FirstOrDefault();

                if (factory != null)
                {
                    database = factory.Name;
                    connection = connectionSetting.Name;
                    ConnectionString = connectionSetting.ConnectionString;
                    configFile = configurationFile;
                    notUsingConfig = false;
                }
            }
            else
            {
                Console.WriteLine("connection is null!");
            }
        }

        private void OutputResults()
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new ArgumentException("Connection String or Name is required \"/connection\"");
            }

            if (string.IsNullOrEmpty(database))
            {
                throw new ArgumentException(
                    "Database Type is required \"/db [db type]\". Available db types is [sqlserver], [sqlite]");
            }

            if (notUsingConfig)
            {
                Console.WriteLine("Using Database {0} and Connection String {1}", database, ConnectionString);
            }
            else
            {
                Console.WriteLine("Using Connection {0} from Configuration file {1}", connection, configFile);
            }
        }

        public string ConnectionString { get; private set; }
    }
}
