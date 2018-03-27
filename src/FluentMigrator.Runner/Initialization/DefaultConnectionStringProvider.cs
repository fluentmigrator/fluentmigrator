#region License
//
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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

namespace FluentMigrator.Runner.Initialization
{
    public class DefaultConnectionStringProvider : IConnectionStringProvider
    {
        private readonly object _syncRoot = new object();
        private string _connectionString;

        public string GetConnectionString(IAnnouncer announcer, string connection, string configPath, string assemblyLocation,
            string database)
        {
            if (_connectionString == null)
            {
                lock (_syncRoot)
                {
                    if (_connectionString == null)
                    {
                        _connectionString = GetConnectionStringFromManager(announcer, connection, configPath,
                            assemblyLocation, database);
                    }
                }
            }

            return _connectionString;
        }

#if NET40 || NET45
        private static string GetConnectionStringFromManager(
            IAnnouncer announcer,
            string connection,
            string configPath,
            string assemblyLocation,
            string database)
        {
            var manager = new NetFramework.ConnectionStringManager(
                new NetFramework.NetConfigManager(),
                announcer,
                connection,
                configPath,
                assemblyLocation,
                database);

            manager.LoadConnectionString();
            return manager.ConnectionString;
        }
#else
        private static string GetConnectionStringFromManager(
            IAnnouncer announcer,
            string connection,
            string configPath,
            string assemblyLocation,
            string database)
        {
            throw new NotImplementedException();
        }
#endif
    }
}
