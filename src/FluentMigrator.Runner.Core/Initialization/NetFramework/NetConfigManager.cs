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
using System.IO;

namespace FluentMigrator.Runner.Initialization.NetFramework
{
    /// <summary>
    /// Provides functionality for managing and loading .NET configuration files in a .NET Framework environment.
    /// </summary>
    /// <remarks>
    /// This class implements the <see cref="FluentMigrator.Runner.Initialization.NetFramework.INetConfigManager"/> interface
    /// and offers methods to load configuration files from specified paths or the machine configuration.
    /// </remarks>
    internal class NetConfigManager
        : INetConfigManager
    {
        /// <summary>
        /// Loads a .NET configuration file from the specified path.
        /// </summary>
        /// <param name="path">The path to the configuration file. If the file does not have a ".config" extension, it will be appended automatically.</param>
        /// <returns>
        /// A <see cref="System.Configuration.Configuration"/> object representing the loaded configuration.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when the <paramref name="path"/> is <c>null</c>, empty, or does not point to an existing file.
        /// </exception>
        public Configuration LoadFromFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                throw new ArgumentException(@"Specified configuration file path does not exist", nameof(path));

            string configFile = path.Trim();

            if (!configFile.EndsWith(".config", StringComparison.InvariantCultureIgnoreCase))
                configFile += ".config";

            var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFile };

            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
        }

        /// <summary>
        /// Loads the machine-level configuration file.
        /// </summary>
        /// <returns>
        /// A <see cref="System.Configuration.Configuration"/> object representing the machine-level configuration.
        /// </returns>
        /// <remarks>
        /// This method retrieves the configuration settings from the machine-level configuration file
        /// using <see cref="System.Configuration.ConfigurationManager.OpenMachineConfiguration"/>.
        /// </remarks>
        public Configuration LoadFromMachineConfiguration()
        {
            return ConfigurationManager.OpenMachineConfiguration();
        }
    }
}
#endif
