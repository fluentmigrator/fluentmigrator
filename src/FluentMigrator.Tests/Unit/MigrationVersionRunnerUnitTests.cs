using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Versioning;
using Xunit;
using Moq;

namespace FluentMigrator.Tests.Unit
{
    public class MigrationVersionRunnerUnitTests
    {
        [Fact]
        public void LoadsCorrectCallingAssembly()
        {
            //mock up the dependencies
            var conventionMock = new Mock<IMigrationConventions>(MockBehavior.Loose);
            var processorMock = new Mock<IMigrationProcessor>(MockBehavior.Loose);
            var loaderMock = new Mock<IMigrationLoader>(MockBehavior.Loose);
            
            var vrunner = new MigrationVersionRunner(conventionMock.Object, processorMock.Object, loaderMock.Object);

            Assert.True(vrunner.MigrationAssembly == Assembly.GetAssembly(typeof(MigrationVersionRunnerUnitTests)));
        }

        [Fact]
        public void HandlesNullMigrationList()
        {
            //mock up the dependencies
            var conventionMock = new Mock<IMigrationConventions>(MockBehavior.Loose);
            var processorMock = new Mock<IMigrationProcessor>(MockBehavior.Loose);
            var loaderMock = new Mock<IMigrationLoader>(MockBehavior.Loose);

            //set migrations to return empty list
            var asm = Assembly.GetAssembly( typeof(MigrationVersionRunnerUnitTests) );
            loaderMock.Setup(x => x.FindMigrationsIn(asm)).Returns<IEnumerable<Migration>>(null);

            var vrunner = new MigrationVersionRunner(conventionMock.Object, processorMock.Object, loaderMock.Object);

            Assert.True(vrunner.Migrations.Count == 0);

            vrunner.UpgradeToLatest(false);

            loaderMock.VerifyAll();
        }
    }
}
