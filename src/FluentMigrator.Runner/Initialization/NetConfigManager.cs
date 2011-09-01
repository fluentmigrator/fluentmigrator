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
            this.connection = connection;
            this.configPath = configPath;
            this.Database = database;
            this.target = target;
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

                var fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = configFile };

                var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

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

        private void ReadConnectionString(ConnectionStringSettings connectionSetting, string configurationFile)
        {
            if (connectionSetting != null)
            {
                var factory = ProcessorFactory.Factories.Where(f => f.IsForProvider(Database)).FirstOrDefault();

                if (factory != null)
                {
                    Database = factory.Name;
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
        public string Database { get; private set; }

    }
}
