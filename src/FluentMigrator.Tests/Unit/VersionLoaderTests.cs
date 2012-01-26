using System.Linq;
using System.Reflection;
using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.VersionTableInfo;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
    public class TestMigrationProcessorOptions : IMigrationProcessorOptions
    {
        public bool PreviewOnly
        {
            get { return false; }
        }

        public int Timeout
        {
            get { return 30; }
        }

        /// <summary>Whether to throw a <see cref="FluentMigrator.Runner.Generators.DatabaseOperationNotSupportedException"/> when a SQL command is not supported by the underlying database type.</summary>
        public bool StrictCompatibility
        {
            get { return false; }
        }

        /// <summary>Whether to imitate database support for some SQL commands that are not supported by the underlying database type.</summary>
        /// <remarks>For example, schema support can be emulated by prefixing the schema name to the table name (<c>`schema`.`table`</c> => <c>`schema_table`</c>).</remarks>
        public bool EmulateCompatibility
        {
            get { return false; }
        }

    }

    [TestFixture]
    public class VersionLoaderTests
    {
        [Test]
        public void CanLoadCustomVersionTableMetaData()
        {
            var runner = new Mock<IMigrationRunner>();
            runner.SetupGet(r => r.Processor.Options).Returns(new TestMigrationProcessorOptions());

            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new VersionLoader(runner.Object, asm, conventions);

            var versionTableMetaData = loader.GetVersionTableMetaData();
            versionTableMetaData.ShouldBeOfType<TestVersionTableMetaData>();
        }

        [Test]
        public void CanLoadDefaultVersionTableMetaData()
        {
            var runner = new Mock<IMigrationRunner>();
            runner.SetupGet(r => r.Processor.Options).Returns(new TestMigrationProcessorOptions());

            var conventions = new MigrationConventions();
            var asm = "s".GetType().Assembly;
            var loader = new VersionLoader(runner.Object, asm, conventions);

            var versionTableMetaData = loader.GetVersionTableMetaData();
            versionTableMetaData.ShouldBeOfType<DefaultVersionTableMetaData>();
        }

        [Test]
        public void DeleteVersionShouldExecuteDeleteDataExpression()
        {
            var processor = new Mock<IMigrationProcessor>();
            var runner = new Mock<IMigrationRunner>();

            runner.SetupGet(r => r.Processor).Returns(processor.Object);

            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new VersionLoader(runner.Object, asm, conventions);

            processor.Setup(p => p.Process(It.Is<DeleteDataExpression>(expression =>
                                                                       expression.SchemaName == loader.VersionTableMetaData.SchemaName
                                                                       && expression.TableName == loader.VersionTableMetaData.TableName
                                                                       && expression.Rows.All(
                                                                           definition =>
                                                                           definition.All(
                                                                               pair =>
                                                                               pair.Key == loader.VersionTableMetaData.ColumnName && pair.Value.Equals(1L))))))
                .Verifiable();

            loader.DeleteVersion(1);

            processor.VerifyAll();
        }

        [Test]
        public void RemoveVersionTableShouldBehaveAsExpected()
        {
            var processor = new Mock<IMigrationProcessor>();
            var runner = new Mock<IMigrationRunner>();

            runner.SetupGet(r => r.Processor).Returns(processor.Object);

            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new VersionLoader(runner.Object, asm, conventions);

            processor.Setup(p => p.Process(It.Is<DeleteTableExpression>(expression =>
                                                                        expression.SchemaName == loader.VersionTableMetaData.SchemaName
                                                                        && expression.TableName == loader.VersionTableMetaData.TableName)))
                .Verifiable();

            processor.Setup(p => p.Process(It.Is<DeleteSchemaExpression>(expression =>
                                                                         expression.SchemaName == loader.VersionTableMetaData.SchemaName)))
                .Verifiable();

            loader.RemoveVersionTable();

            processor.VerifyAll();
        }

        [Test]
        public void UpdateVersionShouldExecuteInsertDataExpression()
        {
            var processor = new Mock<IMigrationProcessor>();
            var runner = new Mock<IMigrationRunner>();

            runner.SetupGet(r => r.Processor).Returns(processor.Object);

            var conventions = new MigrationConventions();
            var asm = Assembly.GetExecutingAssembly();
            var loader = new VersionLoader(runner.Object, asm, conventions);

            processor.Setup(p => p.Process(It.Is<InsertDataExpression>(expression =>
                                                                       expression.SchemaName == loader.VersionTableMetaData.SchemaName
                                                                       && expression.TableName == loader.VersionTableMetaData.TableName
                                                                       && expression.Rows.All(
                                                                           definition =>
                                                                           definition.All(
                                                                               pair =>
                                                                               pair.Key == loader.VersionTableMetaData.ColumnName && pair.Value.Equals(1L))))))
                .Verifiable();

            loader.UpdateVersionInfo(1);

            processor.VerifyAll();
        }
    }
}