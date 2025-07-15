#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

namespace FluentMigrator.Runner.Processors.MySql
{
    public class MySqlDbFactory : ReflectionBasedDbFactory
    {
        private static readonly TestEntry[] _entries =
        {
            new TestEntry("MySql.Data", "MySql.Data.MySqlClient.MySqlClientFactory", () => Type.GetType("MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data")),
            new TestEntry("MySqlConnector", "MySql.Data.MySqlClient.MySqlClientFactory", () => Type.GetType("MySql.Data.MySqlClient.MySqlClientFactory, MySqlConnector")),
            new TestEntry("MySqlConnector", "MySqlConnector.MySqlConnectorFactory", () => Type.GetType("MySqlConnector.MySqlConnectorFactory, MySqlConnector"))
        };

        [Obsolete]
        public MySqlDbFactory()
            : this(serviceProvider: null)
        {
        }

        public MySqlDbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, _entries)
        {
        }
    }
}
