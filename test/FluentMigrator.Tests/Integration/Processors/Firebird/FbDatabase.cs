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

using System.IO;
using System.Threading;

using FirebirdSql.Data.FirebirdClient;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    public class FbDatabase
    {
        public static void CreateDatabase(string connectionString)
        {
            var connectionStringBuilder = new FbConnectionStringBuilder(connectionString);
            if (File.Exists(connectionStringBuilder.Database))
                DropDatabase(connectionString);

            FbConnection.CreateDatabase(connectionString);
        }

        public static void DropDatabase(string connectionString)
        {
            FbConnection.ClearAllPools();

            // Avoid "lock time-out on wait transaction" exception
            var retries = 5;
            while (true)
            {
                try
                {
                    FbConnection.DropDatabase(connectionString);
                    break;
                }
                catch
                {
                    if (--retries == 0)
                        throw;

                    Thread.Sleep(100);
                }
            }
        }
    }
}
