#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using JetBrains.Annotations;

#if NETFRAMEWORK
using FluentMigrator.Runner.Initialization.NetFramework;
using Microsoft.Extensions.Options;
#endif

namespace FluentMigrator.Runner.Initialization
{
    [Obsolete]
    public class DefaultConnectionStringProvider : IConnectionStringProvider
    {
        [CanBeNull]
        [ItemNotNull]
        private readonly IReadOnlyCollection<IConnectionStringReader> _accessors;

        private readonly object _syncRoot = new object();
        private string _connectionString;

        [Obsolete]
        public DefaultConnectionStringProvider()
        {
        }

        public DefaultConnectionStringProvider([NotNull, ItemNotNull] IEnumerable<IConnectionStringReader> accessors)
        {
            _accessors = accessors.ToList();
        }

        public string GetConnectionString(IAnnouncer announcer, string connection, string configPath, string assemblyLocation,
            string database)
        {
            if (_connectionString == null)
            {
                lock (_syncRoot)
                {
                    if (_connectionString == null)
                    {
                        var accessors = _accessors ?? CreateAccessors(assemblyLocation, announcer, configPath).ToList();
                        var result = GetConnectionString(accessors, connection, database);
                        if (string.IsNullOrEmpty(result))
                            result = connection;
                        if (string.IsNullOrEmpty(result))
                            throw new InvalidOperationException("No connection string specified");
                        return _connectionString = result;
                    }
                }
            }

            return _connectionString;
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = "Parameters are required for the full .NET Framework")]
        private static IEnumerable<IConnectionStringReader> CreateAccessors(string assemblyLocation, IAnnouncer announcer, string configPath)
        {
#if NETFRAMEWORK
#pragma warning disable 612
            var options = new AppConfigConnectionStringAccessorOptions()
            {
                ConnectionStringConfigPath = configPath,
            };

            yield return new AppConfigConnectionStringReader(
                new NetConfigManager(),
                assemblyLocation,
                announcer,
                new OptionsWrapper<AppConfigConnectionStringAccessorOptions>(options));
#pragma warning restore 612
#else
            yield break;
#endif
        }

        private static string GetConnectionString(IReadOnlyCollection<IConnectionStringReader> accessors, string connection, string database)
        {
            var result = GetConnectionString(accessors, connection);
            if (result == null)
                result = GetConnectionString(accessors, database);
            if (result == null)
                result = GetConnectionString(accessors, Environment.MachineName);
            return result;
        }

        private static string GetConnectionString(IReadOnlyCollection<IConnectionStringReader> accessors, string connectionStringOrName)
        {
            if (string.IsNullOrEmpty(connectionStringOrName))
                return null;

            foreach (var accessor in accessors.OrderByDescending(x => x.Priority))
            {
                var connectionString = accessor.GetConnectionString(connectionStringOrName);
                if (connectionString != null)
                    return connectionString;
            }

            return null;
        }
    }
}
