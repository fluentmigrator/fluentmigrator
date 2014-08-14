using System;
using System.Configuration;
using System.Text.RegularExpressions;
using FluentMigrator.Exceptions;

namespace FluentMigrator.Runner.Initialization
{
    /// <summary>
    /// Locates connection strings by name in assembly's config file or machine.config
    /// If no connection matches it uses the specified connection string as valid connection
    /// </summary>
    public class ConnectionStringManager
    {
        private readonly IAnnouncer announcer;
        private readonly string assemblyLocation;
        private readonly INetConfigManager configManager;
        private readonly string configPath;
        private readonly string database;
        private string configFile;
        private string connection;
        private Func<string> machineNameProvider = () => Environment.MachineName;
        private bool notUsingConfig;
        private static readonly Regex matchPwd = new Regex("(PWD=|PASSWORD=)([^;]*);", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public ConnectionStringManager(INetConfigManager configManager, IAnnouncer announcer, string connection, string configPath, string assemblyLocation,
                                       string database)
        {
            this.connection = connection;
            this.configPath = configPath;
            this.database = database;
            this.assemblyLocation = assemblyLocation;
            notUsingConfig = true;
            this.configManager = configManager;
            this.announcer = announcer;
        }

        public string ConnectionString { get; private set; }

        public Func<string> MachineNameProvider
        {
            get { return machineNameProvider; }
            set { machineNameProvider = value; }
        }

        public void LoadConnectionString()
        {
            if (notUsingConfig && !string.IsNullOrEmpty(configPath))
                LoadConnectionStringFromConfigurationFile(configManager.LoadFromFile(configPath));

            if (notUsingConfig && !String.IsNullOrEmpty(assemblyLocation))
            {
                string defaultConfigFile = assemblyLocation;

                LoadConnectionStringFromConfigurationFile(configManager.LoadFromFile(defaultConfigFile));
            }

            if (notUsingConfig)
                LoadConnectionStringFromConfigurationFile(configManager.LoadFromMachineConfiguration());

            if (notUsingConfig && !string.IsNullOrEmpty(connection))
                ConnectionString = connection;

            OutputResults();
        }

        private void LoadConnectionStringFromConfigurationFile(Configuration configurationFile)
        {
            var connections = configurationFile.ConnectionStrings.ConnectionStrings;

            if (connections == null || connections.Count <= 0)
                return;

            ConnectionStringSettings connectionString;

            if (string.IsNullOrEmpty(connection))
                connectionString = connections[MachineNameProvider()];
            else
                connectionString = connections[connection];

            ReadConnectionString(connectionString, configurationFile.FilePath);
        }

        private void ReadConnectionString(ConnectionStringSettings connectionSetting, string configurationFile)
        {
            if (connectionSetting == null) return;

            connection = connectionSetting.Name;
            ConnectionString = connectionSetting.ConnectionString;
            configFile = configurationFile;
            notUsingConfig = false;
        }

        private void OutputResults()
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new UndeterminableConnectionException("Unable to resolve any connectionstring using parameters \"/connection\" and \"/configPath\"");

            announcer.Say(notUsingConfig
                              ? string.Format("Using Database {0} and Connection String {1}", database, matchPwd.Replace(ConnectionString,"$1********;"))
                              : string.Format("Using Connection {0} from Configuration file {1}", connection, configFile));
        }
    }
}