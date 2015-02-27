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
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.Runners
{
    [TestFixture]
    public class MigrationProcessorFactoryProviderTests
    {
        private MigrationProcessorFactoryProvider migrationProcessorFactoryProvider;

        [SetUp]
        public void Setup()
        {
            migrationProcessorFactoryProvider = new MigrationProcessorFactoryProvider();
        }

        [Test]
        public void CanRetrieveFactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SQLite");
            Assert.IsTrue(factory.GetType() == typeof(SQLiteProcessorFactory));
        }

        [Test]
        public void CanRetrieveSqlServer2000FactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SqlServer2000");
            Assert.IsTrue(factory.GetType() == typeof(SqlServer2000ProcessorFactory));
        }

        [Test]
        public void CanRetrieveSqlServer2005FactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SqlServer2005");
            Assert.IsTrue(factory.GetType() == typeof(SqlServer2005ProcessorFactory));
        }

        [Test]
        public void CanRetrieveSqlServer2008FactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SqlServer2008");
            Assert.IsTrue(factory.GetType() == typeof(SqlServer2008ProcessorFactory));
        }

        [Test]
        public void CanRetrieveSqlServer2012FactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SqlServer2012");
            Assert.IsTrue(factory.GetType() == typeof(SqlServer2012ProcessorFactory));
        }

        [Test]
        public void CanRetrieveSqlServer2014FactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SqlServer2014");
            Assert.IsTrue(factory.GetType() == typeof(SqlServer2014ProcessorFactory));
        }

        [Test]
        public void RetrievesSqlServerProcessorFactoryIfArgumentIsSqlServer()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SqlServer");
            Assert.IsTrue(factory.GetType() == typeof(SqlServerProcessorFactory));
        }

        [Test]
        public void CanRetrieveSqlServerCeFactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("SqlServerCe");
            Assert.IsTrue(factory.GetType() == typeof(SqlServerCeProcessorFactory));
        }

        [Test]
        public void CanRetrieveOracleFactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("Oracle");
            Assert.IsTrue(factory.GetType() == typeof(OracleProcessorFactory));
        }

        [Test]
        public void CanRetrieveOracleManagedFactoryWithArgumentString()
        {
            IMigrationProcessorFactory factory = migrationProcessorFactoryProvider.GetFactory("OracleManaged");
            Assert.IsTrue(factory.GetType() == typeof(OracleManagedProcessorFactory));
        }

        [Test]
        public void CanRetrieveHanaFactoryWithArgumentString()
        {
            var factory = migrationProcessorFactoryProvider.GetFactory("Hana");
            Assert.IsTrue(factory.GetType() == typeof(HanaProcessorFactory));
        }
    }
}