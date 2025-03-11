using System;
using System.IO;

using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors.MySql;
using FluentMigrator.Tests.Logging;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Integration.Processors.MySql
{
    [TestFixture]
    [Category("Integration")]
    [Category("MySql")]
    public class MySqlProcessorTests
    {
        private ServiceProvider ServiceProvider { get; set; }
        private IServiceScope ServiceScope { get; set; }
        private MySql4Processor Processor { get; set; }

        [Test]
        public void CallingProcessWithPerformDBOperationExpressionWhenInPreviewOnlyModeWillNotMakeDbChanges()
        {
            var output = new StringWriter();

            var sp = CreateProcessorServices(sc => sc
                .AddSingleton<ILoggerProvider>(new TextWriterLoggerProvider(output))
                .ConfigureRunner(r => r.AsGlobalPreview()));

            using (sp)
            {
                using (var scope = sp.CreateScope())
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
                                    command.CommandText = "CREATE TABLE processtesttable (test int NULL) ";
                                    command.Transaction = trans;

                                    command.ExecuteNonQuery();
                                }
                            };

                        processor.Process(expression);

                        tableExists = processor.TableExists("", "processtesttable");
                    }
                    finally
                    {
                        processor.RollbackTransaction();
                    }

                    tableExists.ShouldBeFalse();
                }
            }
        }

        [Test]
        public void CallingExecuteWithPerformDBOperationExpressionWhenInPreviewOnlyModeWillNotMakeDbChanges()
        {
            var output = new StringWriter();

            var sp = CreateProcessorServices(sc => sc
                .AddSingleton<ILoggerProvider>(new TextWriterLoggerProvider(output))
                .ConfigureRunner(r => r.AsGlobalPreview()));

            using (sp)
            {
                using (var scope = sp.CreateScope())
                {
                    var processor = scope.ServiceProvider.GetRequiredService<IMigrationProcessor>();
                    bool tableExists;

                    try
                    {
                        processor.Execute("CREATE TABLE processtesttable (test int NULL) ");

                        tableExists = processor.TableExists("", "processtesttable");
                    }
                    finally
                    {
                        processor.RollbackTransaction();
                    }

                    tableExists.ShouldBeFalse();
                }
            }
        }

        [Test]
        public void CallingDefaultValueExistsReturnsTrueWhenMatches()
        {
            try
            {
                Processor.Execute("CREATE TABLE dftesttable (test int NULL DEFAULT 1) ");
                Processor.DefaultValueExists(null, "dftesttable", "test", 1).ShouldBeTrue();
            }
            finally
            {
                Processor.Execute("DROP TABLE dftesttable");
            }
        }

        [Test]
        public void CallingReadTableDataQuotesTableName()
        {
            try
            {
                Processor.Execute("CREATE TABLE `infrastructure.version` (test int null) ");
                Processor.ReadTableData(null, "infrastructure.version");
            }
            finally
            {
                Processor.Execute("DROP TABLE `infrastructure.version`");
            }
        }

        private static ServiceProvider CreateProcessorServices([CanBeNull] Action<IServiceCollection> initAction)
        {
            IntegrationTestOptions.MySql.IgnoreIfNotEnabled();

            var serivces = ServiceCollectionExtensions.CreateServices()
                .ConfigureRunner(builder => builder.AddMySql4())
                .AddScoped<IConnectionStringReader>(
                    _ => new PassThroughConnectionStringReader(IntegrationTestOptions.MySql.ConnectionString));
            initAction?.Invoke(serivces);
            return serivces.BuildServiceProvider();
        }

        [OneTimeSetUp]
        public void ClassSetUp()
        {
            ServiceProvider = CreateProcessorServices(null);
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
            Processor = ServiceScope.ServiceProvider.GetRequiredService<MySql4Processor>();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceScope?.Dispose();
            Processor?.Dispose();
        }
    }
}
