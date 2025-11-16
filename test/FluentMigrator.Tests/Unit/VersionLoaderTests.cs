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

using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    [Category("Versioning")]
    public class VersionLoaderTests
    {
        [Test]
        public void CanLoadCustomVersionTableMetaData()
        {
            var processor = new Mock<IMigrationProcessor>();
            var generatorAccessor = new Mock<IGeneratorAccessor>();
            var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .WithProcessor(processor)
                .AddScoped(_ => generatorAccessor.Object)
                .AddScoped(_ => ConventionSets.NoSchemaName)
                .AddScoped<IMigrationRunnerConventionsAccessor>(sp => new PassThroughMigrationRunnerConventionsAccessor(new MigrationRunnerConventions()))
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader("No connection"))
                .BuildServiceProvider();

            var loader = serviceProvider.GetRequiredService<IVersionLoader>();

            var versionTableMetaData = loader.GetVersionTableMetaData();
            versionTableMetaData.ShouldBeOfType<TestVersionTableMetaData>();
        }

        [Test]
        public void CanLoadDefaultVersionTableMetaData()
        {
            var asm = "s".GetType().Assembly;

            var processor = new Mock<IMigrationProcessor>();
            var generatorAccessor = new Mock<IGeneratorAccessor>();
            var serviceProvider = ServiceCollectionExtensions.CreateServices(false)
                .WithProcessor(processor)
                .AddScoped(_ => generatorAccessor.Object)
                .AddSingleton<IAssemblySourceItem>(new AssemblySourceItem(asm))
                .AddScoped(_ => ConventionSets.NoSchemaName)
                .AddScoped<IMigrationRunnerConventionsAccessor>(
                    _ => new PassThroughMigrationRunnerConventionsAccessor(new MigrationRunnerConventions()))
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader("No connection"))
                .BuildServiceProvider();

            var loader = serviceProvider.GetRequiredService<IVersionLoader>();

            var versionTableMetaData = loader.GetVersionTableMetaData();
            versionTableMetaData.ShouldBeOfType<DefaultVersionTableMetaData>();
        }

        [Test]
        public void DeleteVersionShouldExecuteDeleteDataExpression()
        {
            var processor = new Mock<IMigrationProcessor>();
            var generatorAccessor = new Mock<IGeneratorAccessor>();
            var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .WithProcessor(processor)
                .AddScoped(_ => generatorAccessor.Object)
                .AddScoped(_ => ConventionSets.NoSchemaName)
                .AddScoped<IMigrationRunnerConventionsAccessor>(
                    _ => new PassThroughMigrationRunnerConventionsAccessor(new MigrationRunnerConventions()))
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader("No connection"))
                .BuildServiceProvider();

            var loader = serviceProvider.GetRequiredService<IVersionLoader>();

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
            var generatorAccessor = new Mock<IGeneratorAccessor>();
            var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .WithProcessor(processor)
                .AddScoped(_ => generatorAccessor.Object)
                .AddScoped(_ => ConventionSets.NoSchemaName)
                .AddScoped<IMigrationRunnerConventionsAccessor>(
                    _ => new PassThroughMigrationRunnerConventionsAccessor(new MigrationRunnerConventions()))
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader("No connection"))
                .BuildServiceProvider();

            var loader = serviceProvider.GetRequiredService<IVersionLoader>();

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
        public void RemoveVersionTableShouldNotRemoveSchemaIfItDidNotOwnTheSchema()
        {
            var processor = new Mock<IMigrationProcessor>();
            var generatorAccessor = new Mock<IGeneratorAccessor>();
            var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .WithProcessor(processor)
                .AddScoped(_ => generatorAccessor.Object)
                .AddScoped(_ => ConventionSets.NoSchemaName)
                .AddScoped<IMigrationRunnerConventionsAccessor>(
                    _ => new PassThroughMigrationRunnerConventionsAccessor(new MigrationRunnerConventions()))
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader("No connection"))
                .BuildServiceProvider();

            var loader = serviceProvider.GetRequiredService<IVersionLoader>();

            ((TestVersionTableMetaData) loader.VersionTableMetaData).OwnsSchema = false;

            processor.Setup(p => p.Process(It.Is<DeleteTableExpression>(expression =>
                                                                        expression.SchemaName == loader.VersionTableMetaData.SchemaName
                                                                        && expression.TableName == loader.VersionTableMetaData.TableName)))
                .Verifiable();

            loader.RemoveVersionTable();

            processor.Verify(p => p.Process(It.IsAny<DeleteSchemaExpression>()), Times.Never());
        }

        [Test]
        public void UpdateVersionShouldExecuteInsertDataExpression()
        {
            var processor = new Mock<IMigrationProcessor>();
            var generatorAccessor = new Mock<IGeneratorAccessor>();
            var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .WithProcessor(processor)
                .AddScoped(_ => generatorAccessor.Object)
                .AddScoped(_ => ConventionSets.NoSchemaName)
                .AddScoped<IMigrationRunnerConventionsAccessor>(
                    _ => new PassThroughMigrationRunnerConventionsAccessor(new MigrationRunnerConventions()))
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader("No connection"))
                .BuildServiceProvider();

            var loader = serviceProvider.GetRequiredService<IVersionLoader>();

            processor.Setup(p => p.Process(It.Is<InsertDataExpression>(expression =>
                                                                       expression.SchemaName == loader.VersionTableMetaData.SchemaName
                                                                       && expression.TableName == loader.VersionTableMetaData.TableName
                                                                       && expression.Rows.Any(
                                                                           definition =>
                                                                           definition.Any(
                                                                               pair =>
                                                                               pair.Key == loader.VersionTableMetaData.ColumnName && pair.Value.Equals(1L))))))
                .Verifiable();

            loader.UpdateVersionInfo(1);

            processor.VerifyAll();
        }

        [Test]
        public void VersionSchemaMigrationOnlyRunOnceEvenIfExistenceChecksReturnFalse()
        {
            var processor = new Mock<IMigrationProcessor>();
            var runner = new Mock<IMigrationRunner>();
            var generatorAccessor = new Mock<IGeneratorAccessor>();
            runner.SetupGet(r => r.Processor).Returns(processor.Object);
            processor.Setup(p => p.SchemaExists(It.IsAny<string>())).Returns(false);

            var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .WithProcessor(processor)
                .AddScoped(_ => generatorAccessor.Object)
                .AddScoped(_ => ConventionSets.NoSchemaName)
                .AddScoped<IMigrationRunnerConventionsAccessor>(
                    _ => new PassThroughMigrationRunnerConventionsAccessor(new MigrationRunnerConventions()))
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader("No connection"))
                .AddScoped<IVersionLoader, VersionLoader>()
                .AddScoped(_ => runner.Object)
                .BuildServiceProvider();

            var loader = (VersionLoader) serviceProvider.GetRequiredService<IVersionLoader>();

            loader.LoadVersionInfo();

            runner.Verify(r => r.Up(loader.VersionSchemaMigration), Times.Once());
        }

        [Test]
        public void VersionMigrationOnlyRunOnceEvenIfExistenceChecksReturnFalse()
        {
            var processor = new Mock<IMigrationProcessor>();
            var runner = new Mock<IMigrationRunner>();
            var generatorAccessor = new Mock<IGeneratorAccessor>();

            runner.SetupGet(r => r.Processor).Returns(processor.Object);
            processor.Setup(
                    p => p.TableExists(new TestVersionTableMetaData().SchemaName, TestVersionTableMetaData.TABLE_NAME))
                .Returns(false);

            var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .WithProcessor(processor)
                .AddScoped(_ => generatorAccessor.Object)
                .AddScoped(_ => ConventionSets.NoSchemaName)
                .AddScoped<IMigrationRunnerConventionsAccessor>(
                    _ => new PassThroughMigrationRunnerConventionsAccessor(new MigrationRunnerConventions()))
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader("No connection"))
                .AddScoped<IVersionLoader, VersionLoader>()
                .AddScoped(_ => runner.Object)
                .BuildServiceProvider();

            var loader = (VersionLoader) serviceProvider.GetRequiredService<IVersionLoader>();

            loader.LoadVersionInfo();

            runner.Verify(r => r.Up(loader.VersionMigration), Times.Once());
        }

        [Test]
        public void VersionUniqueMigrationOnlyRunOnceEvenIfExistenceChecksReturnFalse()
        {
            var processor = new Mock<IMigrationProcessor>();
            var runner = new Mock<IMigrationRunner>();
            var generatorAccessor = new Mock<IGeneratorAccessor>();

            runner.SetupGet(r => r.Processor).Returns(processor.Object);
            processor.Setup(p => p.ColumnExists(new TestVersionTableMetaData().SchemaName, TestVersionTableMetaData.TABLE_NAME, TestVersionTableMetaData.APPLIED_ON_COLUMN_NAME)).Returns(false);

            var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .WithProcessor(processor)
                .AddScoped(_ => generatorAccessor.Object)
                .AddScoped(_ => ConventionSets.NoSchemaName)
                .AddScoped<IMigrationRunnerConventionsAccessor>(
                    _ => new PassThroughMigrationRunnerConventionsAccessor(new MigrationRunnerConventions()))
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader("No connection"))
                .AddScoped<IVersionLoader, VersionLoader>()
                .AddScoped(_ => runner.Object)
                .BuildServiceProvider();

            var loader = (VersionLoader) serviceProvider.GetRequiredService<IVersionLoader>();

            loader.LoadVersionInfo();

            runner.Verify(r => r.Up(loader.VersionUniqueMigration), Times.Once());
        }

        [Test]
        public void VersionDescriptionMigrationOnlyRunOnceEvenIfExistenceChecksReturnFalse()
        {
            var processor = new Mock<IMigrationProcessor>();
            var runner = new Mock<IMigrationRunner>();
            var generatorAccessor = new Mock<IGeneratorAccessor>();

            runner.SetupGet(r => r.Processor).Returns(processor.Object);
            processor.Setup(p => p.ColumnExists(new TestVersionTableMetaData().SchemaName, TestVersionTableMetaData.TABLE_NAME, TestVersionTableMetaData.APPLIED_ON_COLUMN_NAME)).Returns(false);

            var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .WithProcessor(processor)
                .AddScoped(_ => generatorAccessor.Object)
                .AddScoped(_ => ConventionSets.NoSchemaName)
                .AddScoped<IMigrationRunnerConventionsAccessor>(
                    _ => new PassThroughMigrationRunnerConventionsAccessor(new MigrationRunnerConventions()))
                .AddScoped<IConnectionStringReader>(_ => new PassThroughConnectionStringReader("No connection"))
                .AddScoped<IVersionLoader, VersionLoader>()
                .AddScoped(_ => runner.Object)
                .BuildServiceProvider();

            var loader = (VersionLoader) serviceProvider.GetRequiredService<IVersionLoader>();

            loader.LoadVersionInfo();

            runner.Verify(r => r.Up(loader.VersionDescriptionMigration), Times.Once());
        }
    }
}
