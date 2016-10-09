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

using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.Hana;
using FluentMigrator.Runner.Processors.Oracle;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Processors.SqlServer;
using Xunit;

namespace FluentMigrator.Tests.Unit.Runners
{
    public class MigrationProcessorFactoryProviderTests
    {
        private MigrationProcessorFactoryProvider migrationProcessorFactoryProvider;

        [SetUp]
        public void Setup()
        {
            migrationProcessorFactoryProvider = new MigrationProcessorFactoryProvider();
        }

        [Fact]
        public void CanRetrieveFactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SQLite");
            Assert.IsTrue(factory.GetType() == typeof(SQLiteProcessorFactory));
        }

        [Fact]
        public void CanRetrieveSqlServer2000FactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SqlServer2000");
            Assert.IsTrue(factory.GetType() == typeof(SqlServer2000ProcessorFactory));
        }

        [Fact]
        public void CanRetrieveSqlServer2005FactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SqlServer2005");
            Assert.IsTrue(factory.GetType() == typeof(SqlServer2005ProcessorFactory));
        }

        [Fact]
        public void CanRetrieveSqlServer2008FactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SqlServer2008");
            Assert.IsTrue(factory.GetType() == typeof(SqlServer2008ProcessorFactory));
        }

        [Fact]
        public void CanRetrieveSqlServer2012FactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SqlServer2012");
            Assert.IsTrue(factory.GetType() == typeof(SqlServer2012ProcessorFactory));
        }

        [Fact]
        public void CanRetrieveSqlServer2014FactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SqlServer2014");
            Assert.IsTrue(factory.GetType() == typeof(SqlServer2014ProcessorFactory));
        }

        [Fact]
        public void RetrievesSqlServerProcessorFactoryIfArgumentIsSqlServer()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SqlServer");
            Assert.IsTrue(factory.GetType() == typeof(SqlServerProcessorFactory));
        }

        [Fact]
        public void CanRetrieveSqlServerCeFactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SqlServerCe");
            Assert.IsTrue(factory.GetType() == typeof(SqlServerCeProcessorFactory));
        }

        [Fact]
        public void CanRetrieveOracleFactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("Oracle");
            Assert.IsTrue(factory.GetType() == typeof(OracleProcessorFactory));
        }

        [Fact]
        public void CanRetrieveOracleManagedFactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("OracleManaged");
            Assert.IsTrue(factory.GetType() == typeof(OracleManagedProcessorFactory));
        }

        [Fact]
        public void CanRetrieveHanaFactoryWithArgumentString()
        {
            var factory = migrationProcessorFactoryProvider.GetFactory("Hana");
            Assert.IsTrue(factory.GetType() == typeof(HanaProcessorFactory));
        }
    }
}