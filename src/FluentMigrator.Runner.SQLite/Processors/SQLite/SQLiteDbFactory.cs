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

using System;

namespace FluentMigrator.Runner.Processors.SQLite
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteDbFactory : ReflectionBasedDbFactory
    {
        private static readonly TestEntry[] _testEntries =
        {
            new TestEntry("Microsoft.Data.Sqlite", "Microsoft.Data.Sqlite.SqliteFactory", () => Type.GetType("Microsoft.Data.Sqlite.SqliteFactory, Microsoft.Data.Sqlite")),
            new TestEntry("System.Data.SQLite", "System.Data.SQLite.SQLiteFactory", () => Type.GetType("System.Data.SQLite.SQLiteFactory, System.Data.SQLite")),
            new TestEntry("Mono.Data.Sqlite, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756", "Mono.Data.Sqlite.SqliteFactory", () => Type.GetType("Mono.Data.Sqlite.SqliteFactory, Mono.Data.Sqlite")),
        };

        [Obsolete]
        public SQLiteDbFactory()
            : base(_testEntries)
        {
        }

        public SQLiteDbFactory(IServiceProvider serviceProvider)
            : base(serviceProvider, _testEntries)
        {
        }
    }
}
