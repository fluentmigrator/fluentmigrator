using System;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
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
    }

    [TestFixture]
    public class VersionLoaderTests
    {
        [Test]
        public void CanLoadDefaultVersionTableMetaData()
        {
            var runner = new Mock<IMigrationRunner>();
            runner.SetupGet(r=>r.Processor.Options).Returns(new TestMigrationProcessorOptions());

            var conventions = new MigrationConventions();
            var asm = "s".GetType().Assembly;
            var loader = new VersionLoader(runner.Object, asm, conventions);

            var versionTableMetaData = loader.GetVersionTableMetaData();
            versionTableMetaData.ShouldBeOfType<DefaultVersionTableMetaData>();
        }

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

    }
}