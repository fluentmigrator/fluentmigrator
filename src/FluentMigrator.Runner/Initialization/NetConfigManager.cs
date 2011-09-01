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

        public NetConfigManager(string connection, string configPath, string target, string database)
        {
            ConfigPath = configPath;
            Database = database;
            NotUsingConfig = true;
        }

        private string ConfigPath { get; private set; }
        private string Target { get; private set; }
        private string Connection { get; private set; }
        private string ConnectionString { get; private set; }
        private string Database { get; private set; }
        private string ConfigFile { get; private set; }
        private bool NotUsingConfig { get; private set; }

        public void LoadConnectionString()
        {
            if (!String.IsNullOrEmpty(Connection))
            {
                if (NotUsingConfig)
                    LoadFromFile(ConfigPath);

                if (NotUsingConfig)
                {
                    string defaultConfigFile = Environment.CurrentDirectory + Target;

                    LoadFromFile(defaultConfigFile);
                }

                if (NotUsingConfig)
                    LoadFromMachineConfig(false);

                if (NotUsingConfig)
                {
                    if (NotUsingConfig && !string.IsNullOrEmpty(Connection))
                    {
                        ConnectionString = Connection;
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

            if (NotUsingConfig)
            {
                Console.WriteLine("Using Database {0} and Connection String {1}", Database, ConnectionString);
            }
            else
            {
                Console.WriteLine("Using Connection {0} from Configuration file {1}", Connection, ConfigFile);
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
                                               where cnn.Name == Connection
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
                    if (string.IsNullOrEmpty(Connection))
                    {
                        ReadConnectionString(connections[Environment.MachineName], config.FilePath);
                    }
                    else
                    {
                        ReadConnectionString(connections[Connection], config.FilePath);
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
                    Connection = connection.Name;
                    ConnectionString = connection.ConnectionString;
                    ConfigFile = configurationFile;
                    NotUsingConfig = false;
                }
            }
            else
            {
                Console.WriteLine("connection is null!");
            }
        }

    }
}
