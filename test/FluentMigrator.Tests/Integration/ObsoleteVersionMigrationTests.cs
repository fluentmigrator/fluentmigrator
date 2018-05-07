#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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
using System.Reflection;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.Firebird;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Runner.Processors.SqlAnywhere;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Runner.Versioning;
using FluentMigrator.Runner.VersionTableInfo;
using FluentMigrator.Tests.Unit;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration
{
    [TestFixture]
    [Obsolete]
    [Category("Integration")]
    [Category("Firebird")]
    [Category("MySql")]
    [Category("SQLite")]
    [Category("Postgres")]
    [Category("SqlServer2005")]
    [Category("SqlServer2008")]
    [Category("SqlServer2012")]
    [Category("SqlServer2014")]
    public class ObsoleteVersionMigrationTests : ObsoleteIntegrationTestBase
    {
        [Test]
        public void CanUseVersionInfo()
        {
            ExecuteWithSupportedProcessors(processor =>
                {
                    var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), new RunnerContext(new TextWriterAnnouncer(TestContext.Out)) { Namespace = "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3" }, processor);

                    IVersionTableMetaData tableMetaData = new DefaultVersionTableMetaData();

                    //ensure table doesn't exist
                    if (processor.TableExists(tableMetaData.SchemaName, tableMetaData.TableName))
                        runner.Down(new VersionMigration(tableMetaData));

                    runner.Up(new VersionMigration(tableMetaData));
                    processor.TableExists(tableMetaData.SchemaName, tableMetaData.TableName).ShouldBeTrue();

                    runner.Down(new VersionMigration(tableMetaData));
                    processor.TableExists(tableMetaData.SchemaName, tableMetaData.TableName).ShouldBeFalse();
                });
        }

        [Test]
        public void CanUseCustomVersionInfo()
        {
            ExecuteWithSupportedProcessors(processor =>
            {
                var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), new RunnerContext(new TextWriterAnnouncer(TestContext.Out)) { Namespace = "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3" }, processor);

                var tableMetaData = new TestVersionTableMetaData();

                //ensure table doesn't exist
                if (processor.TableExists(tableMetaData.SchemaName, tableMetaData.TableName))
                    runner.Down(new VersionMigration(tableMetaData));

                //ensure schema doesn't exist
                if (processor.SchemaExists(tableMetaData.SchemaName))
                    runner.Down(new VersionSchemaMigration(tableMetaData));


                runner.Up(new VersionSchemaMigration(tableMetaData));
                processor.SchemaExists(tableMetaData.SchemaName).ShouldBeTrue();

                runner.Up(new VersionMigration(tableMetaData));
                processor.TableExists(tableMetaData.SchemaName, tableMetaData.TableName).ShouldBeTrue();

                runner.Down(new VersionMigration(tableMetaData));
                processor.TableExists(tableMetaData.SchemaName, tableMetaData.TableName).ShouldBeFalse();

                runner.Down(new VersionSchemaMigration(tableMetaData));
                processor.SchemaExists(tableMetaData.SchemaName).ShouldBeFalse();
            }, true, typeof(SQLiteProcessor), typeof(MySqlProcessor), typeof(FirebirdProcessor), typeof(SqlAnywhereProcessor));
        }

        [Test]
        public void CanUseCustomVersionInfoDefaultSchema()
        {
            ExecuteWithSupportedProcessors(processor =>
            {
                var runner = new MigrationRunner(Assembly.GetExecutingAssembly(), new RunnerContext(new TextWriterAnnouncer(TestContext.Out)) { Namespace = "FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3" }, processor);

                IVersionTableMetaData tableMetaData = new TestVersionTableMetaData { SchemaName = null };


                //ensure table doesn't exist
                if (processor.TableExists(tableMetaData.SchemaName, tableMetaData.TableName))
                    runner.Down(new VersionMigration(tableMetaData));

                runner.Up(new VersionMigration(tableMetaData));
                processor.TableExists(null, tableMetaData.TableName).ShouldBeTrue();

                runner.Down(new VersionMigration(tableMetaData));
                processor.TableExists(null, tableMetaData.TableName).ShouldBeFalse();
            });
        }
    }
}
