#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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
using System.Data;
using System.IO;
using System.Linq;

using FluentMigrator.Builders.Insert;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Logging;
using FluentMigrator.Runner.Processors.SQLite;
using FluentMigrator.Tests.Helpers;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.SQLite
{
    [TestFixture]
    [Category("Integration")]
    [Category("SQLite")]
    // ReSharper disable once InconsistentNaming
    public class SQLiteProcessorTests
    {
        private Mock<ColumnDefinition> _column;
        private string _columnName;
        private string _tableName;
        private string _tableNameThatMustBeEscaped;

        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private SQLiteProcessor Processor { get; set; }

        [Test]
        public void CanDefaultAutoIncrementColumnTypeToInteger()
        {
            var column = new ColumnDefinition
            {
                Name = "Id",
                IsIdentity = true,
                IsPrimaryKey = true,
                Type = DbType.Int64,
                IsNullable = false
            };

            var expression = new CreateTableExpression { TableName = _tableName };
            expression.Columns.Add(column);

            Processor.Process(expression);
            Processor.TableExists(null, _tableName).ShouldBeTrue();
            Processor.ColumnExists(null, _tableName, "Id").ShouldBeTrue();
        }

        [Test]
        public void CanCreateTableExpression()
        {
            var expression = new CreateTableExpression { TableName = _tableName };
            expression.Columns.Add(_column.Object);

            Processor.Process(expression);
            Processor.TableExists(null, _tableName).ShouldBeTrue();
            Processor.ColumnExists(null, _tableName, _columnName).ShouldBeTrue();
        }

        [Test]
        public void IsEscapingTableNameCorrectlyOnTableCreate()
        {
            var expression = new CreateTableExpression { TableName = _tableNameThatMustBeEscaped };
            expression.Columns.Add(_column.Object);

            Processor.Process(expression);
        }

        [Test]
        public void IsEscapingTableNameCorrectlyOnReadTableData()
        {
            var expression = new CreateTableExpression { TableName = _tableNameThatMustBeEscaped };
            expression.Columns.Add(_column.Object);
            Processor.Process(expression);
            Processor.ReadTableData(null, _tableNameThatMustBeEscaped).Tables.Count.ShouldBe(1);
        }

        [Test]
        public void IsEscapingTableNameCorrectlyOnTableExists()
        {
            var expression = new CreateTableExpression { TableName = _tableNameThatMustBeEscaped };
            expression.Columns.Add(_column.Object);
            Processor.Process(expression);
            Processor.TableExists(null, _tableNameThatMustBeEscaped).ShouldBeTrue();
        }

        [Test]
        public void IsEscapingTableNameCorrectlyOnColumnExists()
        {
            const string columnName = "123ColumnName";

            var expression = new CreateTableExpression { TableName = _tableNameThatMustBeEscaped };
            expression.Columns.Add(new ColumnDefinition() { Name = "123ColumnName", Type = DbType.AnsiString, IsNullable = true });

            Processor.Process(expression);
            Processor.ColumnExists(null, _tableNameThatMustBeEscaped, columnName).ShouldBeTrue();
        }

        [Test]
        public void PrimaryKeyNonIdentityColumnsSupported()
        {
            var expression = new CreateTableExpression { TableName = _tableName };
            expression.Columns.Add(new ColumnDefinition {Name = "Id", Type = DbType.Int32, IsPrimaryKey = false, IsIdentity = true, IsNullable = false });
            expression.Columns.Add(new ColumnDefinition {Name = "Key1", Type = DbType.String, IsPrimaryKey = true, IsIdentity = false, IsNullable = false });
            expression.Columns.Add(new ColumnDefinition {Name = "Key2", Type = DbType.String, IsPrimaryKey = true, IsIdentity = false, IsNullable = false });

            Processor.Process(expression);
            Processor.ColumnExists(null, _tableName, "Id").ShouldBeTrue();
            Processor.ColumnExists(null, _tableName, "Key1").ShouldBeTrue();
            Processor.ColumnExists(null, _tableName, "Key2").ShouldBeTrue();
        }

        [Test]
        public void AGuidCanBeInsertedAndReadAgain([Values] bool binaryGuid)
        {
            using (var serviceProvider = CreateProcessorServices(null, binaryGuid, false))
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var processor = scope.ServiceProvider.GetRequiredService<SQLiteProcessor>();

                    var originalGuid = new Guid("B5BA91DD-2D1D-4754-81EE-03F82B8EEFD8");

                    using (var sqLiteTestTable = new SQLiteTestTable(processor, null, "GUID UNIQUEIDENTIFIER")) {
                        var insertDataExpression = new InsertDataExpression { TableName = sqLiteTestTable.Name };
                        var builder = new InsertDataExpressionBuilder(insertDataExpression);
                        builder.Row(new { GUID = originalGuid });
                        processor.Process(insertDataExpression);

                        using (var dataSet = processor.ReadTableData(null, sqLiteTestTable.Name))
                        {
                            using (var dataTable = dataSet.Tables[0])
                            {
                                var dataRow = dataTable.Rows.Cast<DataRow>().Single();
                                var value = dataRow["GUID"];
                                var actualGuid = binaryGuid ? new Guid((byte[])value) : new Guid((string)value);
                                Assert.That(actualGuid, Is.EqualTo(originalGuid));
                            }
                        }
                    }
                }
            }
        }

        [Test]
        public void CallingProcessWithPerformDBOperationExpressionWhenInPreviewOnlyModeWillNotMakeDbChanges()
        {
            var output = new StringWriter();

            var serviceProvider = CreateProcessorServices(
                services => services
                    .AddSingleton<ILoggerProvider>(new SqlScriptFluentMigratorLoggerProvider(output))
                    .ConfigureRunner(r => r.AsGlobalPreview()),
                false,
                false);
            using (serviceProvider)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var processor = scope.ServiceProvider.GetRequiredService<IMigrationProcessor>();

                    bool tableExists;

                    try
                    {
                        var expression =
                            new PerformDBOperationExpression
                            {
                                Operation = (con, trans) =>
                                {
                                    var command = con.CreateCommand();
                                    command.CommandText = "CREATE TABLE ProcessTestTable (test int NULL) ";
                                    command.Transaction = trans;

                                    command.ExecuteNonQuery();
                                }
                            };

                        processor.Process(expression);

                        tableExists = processor.TableExists("", "ProcessTestTable");
                    }
                    finally
                    {
                        processor.RollbackTransaction();
                    }

                    tableExists.ShouldBeFalse();
                }
            }

            Assert.That(output.ToString(), Does.Contain(@"/* Performing DB Operation */"));
        }

        private ServiceProvider CreateProcessorServices([CanBeNull] Action<IServiceCollection> initAction, bool binaryGuid, bool useStrictTables)
        {
            IntegrationTestOptions.SQLite.IgnoreIfNotEnabled();

            var services = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(r => r.AddSQLite(binaryGuid, useStrictTables))
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader("Data Source=:memory:;Pooling=False;")); // Just use in-memory DB

            initAction?.Invoke(services);

            return services.BuildServiceProvider();
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            ServiceProvider = CreateProcessorServices(initAction: null, binaryGuid: false, useStrictTables: false);

            _column = new Mock<ColumnDefinition>();
            _tableName = "NewTable";
            _tableNameThatMustBeEscaped = "123NewTable";
            _columnName = "ColumnName";
            _column.SetupGet(c => c.Name).Returns(_columnName);
            _column.SetupGet(c => c.IsNullable).Returns(true);
            _column.SetupGet(c => c.Type).Returns(DbType.Int32);
        }

        [OneTimeTearDown]
        public void ClassTearDown()
        {
            ServiceProvider?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            ServiceScope = ServiceProvider.CreateScope();
            Processor = ServiceScope.ServiceProvider.GetRequiredService<SQLiteProcessor>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();
        }
    }
}
