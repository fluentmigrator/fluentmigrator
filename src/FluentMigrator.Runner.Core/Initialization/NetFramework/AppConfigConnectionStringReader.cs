#region License
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

#if NETFRAMEWORK

using System;
using System.Configuration;
using System.Linq;

using FluentMigrator.Runner.Logging;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Initialization.NetFramework
{
    /// <summary>
    /// A <see cref="IConnectionStringReader"/> implementation that uses the app or machine config
    /// </summary>
    [Obsolete]
    public class AppConfigConnectionStringReader : IConnectionStringReader
    {
        [NotNull]
        private readonly IPasswordMaskUtility _passwordMaskUtility = new PasswordMaskUtility();

        [NotNull]
        private readonly INetConfigManager _configManager;

        [NotNull]
        private readonly ILogger _logger;

        [NotNull]
        private readonly AppConfigConnectionStringAccessorOptions _options;

        [CanBeNull]
        private readonly string _assemblyLocation;

        [CanBeNull]
        private ConnectionInfo _connectionInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppConfigConnectionStringReader"/> class.
        /// </summary>
        /// <param name="configManager">
        /// The <see cref="INetConfigManager"/> instance used to access .NET configuration mechanisms.
        /// </param>
        /// <param name="assemblySource">
        /// The <see cref="IAssemblySource"/> instance providing access to assemblies for determining the assembly location.
        /// </param>
        /// <param name="logger">
        /// The <see cref="ILogger{TCategoryName}"/> instance for logging messages.
        /// </param>
        /// <param name="options">
        /// The options for configuring the <see cref="AppConfigConnectionStringReader"/>.
        /// </param>
        /// <remarks>
        /// This constructor is marked as <see cref="ObsoleteAttribute"/> and may be removed in future versions.
        /// </remarks>
        public AppConfigConnectionStringReader(
            [NotNull] INetConfigManager configManager,
            [NotNull] IAssemblySource assemblySource,
            [NotNull] ILogger<AppConfigConnectionStringReader> logger,
            [NotNull] IOptions<AppConfigConnectionStringAccessorOptions> options)
        {
            _configManager = configManager;
            _logger = logger;
            _options = options.Value;
            var assemblies = assemblySource.Assemblies;
            var singleAssembly = assemblies.Count == 1 ? assemblies.Single() : null;
            _assemblyLocation = singleAssembly != null ? singleAssembly.Location : string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppConfigConnectionStringReader"/> class.
        /// </summary>
        /// <param name="configManager">The configuration manager used to access .NET configuration sections.</param>
        /// <param name="assemblyLocation">The location of the assembly to be used for configuration resolution.</param>
        /// <param name="announcer">The announcer used for logging messages.</param>
        /// <param name="options">The options for configuring the connection string reader.</param>
        /// <remarks>
        /// This constructor is marked as <see cref="ObsoleteAttribute"/> and may be removed in future versions.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="configManager"/>, <paramref name="assemblyLocation"/>, <paramref name="announcer"/>, 
        /// or <paramref name="options"/> is <c>null</c>.
        /// </exception>
        [Obsolete]
        public AppConfigConnectionStringReader(
            [NotNull] INetConfigManager configManager,
            [NotNull] string assemblyLocation,
            [NotNull] IAnnouncer announcer,
            [NotNull] IOptions<AppConfigConnectionStringAccessorOptions> options)
        {
            _configManager = configManager;
            _logger = new AnnouncerFluentMigratorLogger(announcer);
            _options = options.Value;
            _assemblyLocation = assemblyLocation;
        }

        /// <inheritdoc />
        public int Priority { get; } = 0;

        /// <inheritdoc />
        public string GetConnectionString(string connectionStringOrName)
        {
            if (_connectionInfo != null)
            {
                return _connectionInfo.ConnectionString;
            }

            var result = LoadConnectionString(connectionStringOrName, _assemblyLocation);
            OutputResults(result);

            _connectionInfo = result;

            return result?.ConnectionString ?? connectionStringOrName;
        }

        /// <summary>
        /// Loads a connection string based on the provided name or value and the specified assembly location.
        /// </summary>
        /// <param name="connectionStringOrName">
        /// The connection string or the name of the connection string to load. Can be <c>null</c>.
        /// </param>
        /// <param name="assemblyLocation">
        /// The location of the assembly to use for loading the connection string. Can be <c>null</c>.
        /// </param>
        /// <returns>
        /// A <see cref="ConnectionInfo"/> object containing the loaded connection string and related details,
        /// or <c>null</c> if no connection string could be resolved.
        /// </returns>
        [CanBeNull]
        private ConnectionInfo LoadConnectionString([CanBeNull] string connectionStringOrName, [CanBeNull] string assemblyLocation)
        {
            ConnectionInfo result = null;

            if (!string.IsNullOrEmpty(_options.ConnectionStringConfigPath))
            {
                result = LoadConnectionStringFromConfigurationFile(connectionStringOrName, _configManager.LoadFromFile(_options.ConnectionStringConfigPath));
            }

            if (result == null && !string.IsNullOrEmpty(assemblyLocation))
            {
                result = LoadConnectionStringFromConfigurationFile(connectionStringOrName, _configManager.LoadFromFile(assemblyLocation));
            }

            if (result == null)
            {
                result = LoadConnectionStringFromConfigurationFile(connectionStringOrName, _configManager.LoadFromMachineConfiguration());
            }

            if (result == null && !string.IsNullOrEmpty(connectionStringOrName))
            {
                result = new ConnectionInfo(name: null, connectionStringOrName, source: null);
            }

            return result;
        }

        /// <summary>
        /// Loads a connection string from the specified configuration file.
        /// </summary>
        /// <param name="connectionStringName">
        /// The name of the connection string to retrieve. If <c>null</c> or empty, 
        /// the connection string associated with the machine name is used.
        /// </param>
        /// <param name="configurationFile">
        /// The configuration file from which to load the connection string.
        /// </param>
        /// <returns>
        /// A <see cref="ConnectionInfo"/> object containing the connection string details, 
        /// or <c>null</c> if the connection string could not be found.
        /// </returns>
        [CanBeNull]
        private ConnectionInfo LoadConnectionStringFromConfigurationFile([CanBeNull] string connectionStringName, [NotNull] Configuration configurationFile)
        {
            var connections = configurationFile.ConnectionStrings.ConnectionStrings;

            if (connections == null || connections.Count <= 0)
                return null;

            ConnectionStringSettings connectionString;

            if (string.IsNullOrEmpty(connectionStringName))
                connectionString = connections[_options.MachineName ?? Environment.MachineName];
            else
                connectionString = connections[connectionStringName];

            return ReadConnectionString(connectionString, configurationFile.FilePath);
        }

        /// <summary>
        /// Reads a connection string from the provided <see cref="ConnectionStringSettings"/> and configuration file path.
        /// </summary>
        /// <param name="connectionSetting">
        /// The <see cref="ConnectionStringSettings"/> instance containing the connection string information.
        /// </param>
        /// <param name="configurationFile">
        /// The path to the configuration file from which the connection string is being read.
        /// </param>
        /// <returns>
        /// A <see cref="ConnectionInfo"/> instance containing the connection string details, or <c>null</c> if the
        /// <paramref name="connectionSetting"/> is <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This method is marked as <see cref="PureAttribute"/>, indicating it does not modify the state of the object.
        /// </remarks>
        [Pure]
        [CanBeNull]
        private ConnectionInfo ReadConnectionString(
            [CanBeNull] ConnectionStringSettings connectionSetting,
            string configurationFile)
        {
            if (connectionSetting == null)
                return null;
            return new ConnectionInfo(connectionSetting.Name, connectionSetting.ConnectionString, configurationFile);
        }

        /// <summary>
        /// Outputs the results of the connection string resolution process to the logger.
        /// </summary>
        /// <param name="info">The <see cref="ConnectionInfo"/> object containing the resolved connection string and its metadata.
        /// If <c>null</c>, an error message will be logged.</param>
        /// <remarks>
        /// This method logs the connection string details, masking sensitive information such as passwords.
        /// If the connection string cannot be resolved, an error message is logged.
        /// </remarks>
        private void OutputResults(ConnectionInfo info)
        {
            if (info == null)
            {
                _logger.LogError("Unable to resolve any ConnectionString using parameters \"/connection\" and \"/configPath\"");
                return;
            }

            var connectionString = _passwordMaskUtility.ApplyMask(info.ConnectionString);
            string message;
            if (string.IsNullOrEmpty(info.Source))
            {
                if (string.IsNullOrEmpty(info.Name))
                {
                    message = $"Using connection string {connectionString}";
                }
                else
                {
                    message = $"Using database {info.Name} and connection string {connectionString}";
                }
            }
            else
            {
                message = $"Using connection {info.Name} from configuration file {info.Source}";
            }

            _logger.LogSay(message);
        }

        /// <summary>
        /// Represents information about a database connection, including the connection string, 
        /// its name, and the source from which it was loaded.
        /// </summary>
        private class ConnectionInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ConnectionInfo"/> class.
            /// </summary>
            /// <param name="name">
            /// The name of the connection string. Can be <c>null</c> if the connection string does not have a name.
            /// </param>
            /// <param name="connectionString">
            /// The connection string used to connect to the database. Cannot be <c>null</c>.
            /// </param>
            /// <param name="source">
            /// The source from which the connection string was loaded. Can be <c>null</c> if the source is not specified.
            /// </param>
            /// <exception cref="ArgumentNullException">
            /// Thrown when <paramref name="connectionString"/> is <c>null</c>.
            /// </exception>
            public ConnectionInfo([CanBeNull] string name, [NotNull] string connectionString, [CanBeNull] string source)
            {
                Name = name;
                ConnectionString = connectionString;
                Source = source;
            }

            /// <summary>
            /// Gets the name of the connection string.
            /// </summary>
            /// <value>
            /// The name of the connection string, or <c>null</c> if the connection string does not have a name.
            /// </value>
            [CanBeNull]
            public string Name { get; }

            /// <summary>
            /// Gets the connection string used to connect to the database.
            /// </summary>
            /// <value>
            /// A non-null string representing the connection string.
            /// </value>
            /// <exception cref="ArgumentNullException">
            /// Thrown when attempting to set the property to <c>null</c>.
            /// </exception>
            [NotNull]
            public string ConnectionString { get; }

            /// <summary>
            /// Gets the source from which the connection string was loaded.
            /// </summary>
            /// <value>
            /// The source of the connection string, such as a configuration file path. 
            /// Can be <c>null</c> if the source is not specified.
            /// </value>
            [CanBeNull]
            public string Source { get; }
        }
    }
}
#endif
