using System;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Linq;
using FluentMigrator.Runner.Processors;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Understands the surrounding environment and delivers the proper environment settings
    /// </summary>
    public class NetConfigManager
    {
        private string configPath;
        private string target;
        private string connection;
        private string configFile;
        private bool notUsingConfig;

        public NetConfigManager(string connection, string configPath, string target, string database)
        {
            this.configPath = configPath;
            this.Database = database;
            this.notUsingConfig = true;
        }

        public void LoadConnectionString()
        {
            if (!String.IsNullOrEmpty(connection))
            {
                if (notUsingConfig)
                    LoadFromFile(configPath);

                if (notUsingConfig)
                {
                    string defaultConfigFile = Environment.CurrentDirectory + target;

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

            if (string.IsNullOrEmpty(Database))
            {
                throw new ArgumentException(
                    "Database Type is required \"/db [db type]\". Available db types is [sqlserver], [sqlite]");
            }

            if (notUsingConfig)
            {
                Console.WriteLine("Using Database {0} and Connection String {1}", Database, ConnectionString);
            }
            else
            {
                Console.WriteLine("Using Connection {0} from Configuration file {1}", connection, configFile);
            }
        }

        private void LoadFromMachineConfig(bool useDefault)
        {
            ConnectionStringSettings machineConnectionString = null;

            if (ConfigurationManager.ConnectionStrings.Count > 0)
            {
                if (useDefault)
                    machineConnectionString = ConfigurationManager.ConnectionStrings[0];
                else
                    machineConnectionString = (from cnn in ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>()
                                               where cnn.Name == connection
                                               select cnn).FirstOrDefault();

                ReadConnectionString(machineConnectionString, "machine.config");
            }
        }

        private void LoadFromFile(string path)
        {
            if (!String.IsNullOrEmpty(path) && File.Exists(path))
            {
                string configFile = path.Trim();

                if (!configFile.EndsWith(".config", StringComparison.InvariantCultureIgnoreCase))
                    configFile += ".config";

                var config = ConfigurationManager.OpenExeConfiguration(configFile);
                var connections = config.ConnectionStrings.ConnectionStrings;

                if (connections != null && connections.Count > 0)
                {
                    if (string.IsNullOrEmpty(connection))
                    {
                        ReadConnectionString(connections[Environment.MachineName], config.FilePath);
                    }
                    else
                    {
                        ReadConnectionString(connections[connection], config.FilePath);
                    }
                }
            }
        }

        private void ReadConnectionString(ConnectionStringSettings connection, string configurationFile)
        {
            if (connection != null)
            {
                var factory = ProcessorFactory.Factories.Where(f => f.IsForProvider(Database)).FirstOrDefault();

                if (factory != null)
                {
                    Database = factory.Name;
                    connection = connection.Name;
                    ConnectionString = connection.ConnectionString;
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
        public string Database { get; private set; }

    }
}
