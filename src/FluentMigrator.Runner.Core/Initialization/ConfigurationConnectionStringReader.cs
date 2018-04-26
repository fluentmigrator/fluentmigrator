#region License
// Copyright (c) 2018, FluentMigrator Project
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

using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner.Initialization
{
    public class ConfigurationConnectionStringReader : IConnectionStringReader
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationConnectionStringReader"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider to get the configuration interface from</param>
        public ConfigurationConnectionStringReader(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public int Priority { get; } = 100;

        /// <inheritdoc />
        public string GetConnectionString(string connectionStringName)
        {
            if (_serviceProvider == null)
                return null;
            var cfg = _serviceProvider.GetService<IConfigurationRoot>()
             ?? _serviceProvider.GetService<IConfiguration>();
            if (cfg == null)
                return null;
            return cfg.GetConnectionString(connectionStringName) ?? cfg.GetConnectionString(connectionStringName);
        }
    }
}
