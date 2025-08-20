#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

#if NETFRAMEWORK
using System;
using System.Configuration;
using System.Text.RegularExpressions;

using FluentMigrator.Exceptions;

using Microsoft.Extensions.Logging;

namespace FluentMigrator.Runner.Initialization.NetFramework
{
    /// <summary>
    /// Locates connection strings by name in assembly's config file or machine.config
    /// If no connection matches it uses the specified connection string as valid connection
    /// </summary>
    internal class ConnectionStringManager
    {
        private static readonly Regex _matchPwd = new Regex("(PWD=|PASSWORD=)([^;]*);", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly ILogger _logger;
        private readonly string _assemblyLocation;
        private readonly INetConfigManager _configManager;
        private readonly string _configPath;
        private readonly string _database;
        private string _configFile;
        private string _connection;
        private Func<string> _machineNameProvider = () => Environment.MachineName;
        private bool _notUsingConfig;

        public ConnectionStringManager(INetConfigManager configManager, ILogger<ConnectionStringManager> logger, string connection, string configPath, string assemblyLocation,
                                       string database)
        {
            _connection = connection;
            _configPath = configPath;
            _database = database;
            _assemblyLocation = assemblyLocation;
            _notUsingConfig = true;
            _configManager = configManager;
            _logger = logger;
        }

        public string ConnectionString { get; private set; }

        public Func<string> MachineNameProvider
        {
            get => _machineNameProvider;
            set => _machineNameProvider = value;
        }

        public void LoadConnectionString()
        {
            if (_notUsingConfig && !string.IsNullOrEmpty(_configPath))
                LoadConnectionStringFromConfigurationFile(_configManager.LoadFromFile(_configPath));

            if (_notUsingConfig && !string.IsNullOrEmpty(_assemblyLocation))
            {
                string defaultConfigFile = _assemblyLocation;

                LoadConnectionStringFromConfigurationFile(_configManager.LoadFromFile(defaultConfigFile));
            }

            if (_notUsingConfig)
                LoadConnectionStringFromConfigurationFile(_configManager.LoadFromMachineConfiguration());

            if (_notUsingConfig && !string.IsNullOrEmpty(_connection))
                ConnectionString = _connection;

            OutputResults();
        }

        private void LoadConnectionStringFromConfigurationFile(Configuration configurationFile)
        {
            var connections = configurationFile.ConnectionStrings.ConnectionStrings;

            if (connections == null || connections.Count <= 0)
                return;

            ConnectionStringSettings connectionString;

            if (string.IsNullOrEmpty(_connection))
                connectionString = connections[MachineNameProvider()];
            else
                connectionString = connections[_connection];

            ReadConnectionString(connectionString, configurationFile.FilePath);
        }

        private void ReadConnectionString(ConnectionStringSettings connectionSetting, string configurationFile)
        {
            if (connectionSetting == null) return;

            _connection = connectionSetting.Name;
            ConnectionString = connectionSetting.ConnectionString;
            _configFile = configurationFile;
            _notUsingConfig = false;
        }

        private void OutputResults()
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new UndeterminableConnectionException("Unable to resolve any connectionstring using parameters \"/connection\" and \"/configPath\"");

            _logger.LogSay(
                _notUsingConfig
                    ? $"Using Database {_database} and Connection String {_matchPwd.Replace(ConnectionString, "$1********;")}"
                    : $"Using Connection {_connection} from Configuration file {_configFile}");
        }
    }
}
#endif
