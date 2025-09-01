#region License
// Copyright (c) 2018, Fluent Migrator Project
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

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Provides extension methods for configuring an in-process migration runner builder with support for various database systems.
    /// </summary>
    public static class FluentMigrationRunnerBuilderExtensions
    {
        /// <summary>
        /// Add all database services to the migration runner
        /// </summary>
        /// <param name="builder">The migration runner builder</param>
        /// <returns>The migration runner builder</returns>
        public static IMigrationRunnerBuilder AddAllDatabases(this IMigrationRunnerBuilder builder)
        {
            return builder
                .AddDb2()
                .AddDb2ISeries()
                .AddDotConnectOracle()
                .AddDotConnectOracle12C()
                .AddFirebird()
                .AddHana()
#if NETFRAMEWORK
                .AddJet()
#endif
                .AddMySql4()
                .AddMySql5()
                .AddMySql8()
                .AddOracle()
                .AddOracle12C()
                .AddOracleManaged()
                .AddOracle12CManaged()
                .AddPostgres()
                .AddPostgres92()
                .AddRedshift()
                .AddSnowflake()
                .AddSQLite()
                .AddSqlServer()
                .AddSqlServer2000()
                .AddSqlServer2005()
                .AddSqlServer2008()
                .AddSqlServer2012()
                .AddSqlServer2014()
                .AddSqlServer2016()
                ;
        }
    }
}
