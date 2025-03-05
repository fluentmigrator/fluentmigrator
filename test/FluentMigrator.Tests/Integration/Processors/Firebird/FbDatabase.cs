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

using System.Threading;

using FirebirdSql.Data.FirebirdClient;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Tests.Integration.Processors.Firebird
{
    public class FbDatabase
    {
        public static void CreateDatabase(string connectionString)
        {
            try
            {
                DropDatabase(connectionString);
            }
            catch
            {
                // Ignore
            }

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

        public static IServiceCollection CreateFirebirdServices(FirebirdLibraryProber prober, out TemporaryDatabase temporaryDatabase)
        {
            var services = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(builder => builder.AddFirebird());

            var tempDb = new TemporaryDatabase(IntegrationTestOptions.Firebird, prober);

            services.AddScoped<IConnectionStringReader>(_ =>
                new PassThroughConnectionStringReader(tempDb.ConnectionString)
            );

            temporaryDatabase = tempDb;

            return services;
        }
    }
}
