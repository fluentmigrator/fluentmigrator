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

        public ConnectionStringManager(string connection, string configPath, string assemblyLocation, string database)
        {
            this.connection = connection;
            this.configPath = configPath;
            this.database = database;
            this.assemblyLocation = assemblyLocation;
            notUsingConfig = true;
        }

        public void LoadConnectionString()
        {
            if (!String.IsNullOrEmpty(connection))
            {
                if (notUsingConfig)
                    LoadFromFile(configPath);

                if (notUsingConfig)
                {
                    string defaultConfigFile = assemblyLocation;

                    LoadFromFile(defaultConfigFile);
                }

                if (notUsingConfig)
                    LoadFromMachineConfig(false);

                if (notUsingConfig)
                {
                    if (notUsingConfig && !string.IsNullOrEmpty(connection))
                    {
                        ConnectionString = connection;
                    }
                }
            }
            else
                LoadFromMachineConfig(true);

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

        private void LoadFromMachineConfig(bool useDefault)
        {
            if (ConfigurationManager.ConnectionStrings.Count == 0)
                return;

            ConnectionStringSettings machineConnectionString;
            if (useDefault)
                machineConnectionString = ConfigurationManager.ConnectionStrings[0];
            else
                machineConnectionString = (from cnn in ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>()
                                           where cnn.Name == connection
                                           select cnn).FirstOrDefault();

            ReadConnectionString(machineConnectionString, "machine.config");
        }

        private void LoadFromFile(string path)
        {
            if (String.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            string configFile = path.Trim();

            if (!configFile.EndsWith(".config", StringComparison.InvariantCultureIgnoreCase))
                configFile += ".config";

            var fileMap = new ExeConfigurationFileMap {ExeConfigFilename = configFile};

            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            var connections = config.ConnectionStrings.ConnectionStrings;

            if (connections == null || connections.Count <= 0)
                return;

            if (string.IsNullOrEmpty(connection))
            {
                ReadConnectionString(connections[Environment.MachineName], config.FilePath);
            }
            else
            {
                ReadConnectionString(connections[connection], config.FilePath);
            }
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

        public string ConnectionString { get; private set; }
    }
}
