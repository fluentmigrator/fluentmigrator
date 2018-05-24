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
    internal class NetConfigManager
        : INetConfigManager
    {
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

        public Configuration LoadFromMachineConfiguration()
        {
            return ConfigurationManager.OpenMachineConfiguration();
        }
    }
}
#endif
