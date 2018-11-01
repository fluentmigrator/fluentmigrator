using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.SQLite;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.SQLite
{
    [TestFixture]
    [Category("Integration")]
    [Category("SQLite2")]
    public class SQLite2ProcessorTests
    {
        private Mock<ColumnDefinition> _column;
        private string _columnName;
        private string _tableName;

        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private SQLite2Processor Processor { get; set; }

        [Test]
        public void SQLite2Int64KeyShouldBeInteger()
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
            Processor.GetColumnType(null, _tableName, "Id").ShouldBe("INTEGER");
        }

        [Test]
        public void EmptyColumnNameShouldFail()
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
            Processor.ColumnExists(null, _tableName, "NonExistentColumn").ShouldBeFalse();
        }



        private ServiceProvider CreateProcessorServices([CanBeNull] Action<IServiceCollection> initAction)
        {
            if (!IntegrationTestOptions.SQLite2.IsEnabled)
                Assert.Ignore();

            var serivces = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(r => r.AddSQLite2())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.SQLite2.ConnectionString));

            initAction?.Invoke(serivces);

            return serivces.BuildServiceProvider();
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            ServiceProvider = CreateProcessorServices(initAction: null);

            _column = new Mock<ColumnDefinition>();
            _tableName = "NewTable";
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
            Processor = ServiceScope.ServiceProvider.GetRequiredService<SQLite2Processor>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
        }
    }
}
