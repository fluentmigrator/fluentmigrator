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
using System.IO;

using Microsoft.Data.Sqlite;

namespace FluentMigrator.Example.Migrator
{
    internal static partial class Program
    {
        static void Main(string[] args)
        {
            // Configure the DB connection
            var dbFileName = Path.Combine(AppContext.BaseDirectory, "test.db");
            var csb = new SqliteConnectionStringBuilder
            {
                DataSource = dbFileName,
                Mode = SqliteOpenMode.ReadWriteCreate
            };

            // The poor mans command line parser
            var useLegacyMode = args.Length > 0 && args[0] == "--mode=legacy";

            if (!useLegacyMode)
            {
                Console.WriteLine(@"Using dependency injection");
                RunWithServices(csb.ConnectionString);
            }
            else
            {
                Console.WriteLine(@"Using legacy mode");
                RunInLegacyMode(csb.ConnectionString);
            }
        }
    }
}
