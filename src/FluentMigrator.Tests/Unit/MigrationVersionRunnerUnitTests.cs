using System.Collections.Generic;
using System.Reflection;
using FluentMigrator.Runner;
using Moq;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit
{
	public class MigrationVersionRunnerUnitTests
	{
		[Test]
		public void LoadsCorrectCallingAssembly()
		{
			//mock up the dependencies
			var conventionMock = new Mock<IMigrationConventions>(MockBehavior.Loose);
			var processorMock = new Mock<IMigrationProcessor>(MockBehavior.Loose);
			var loaderMock = new Mock<IMigrationLoader>(MockBehavior.Loose);

			var vrunner = new MigrationVersionRunner(conventionMock.Object, processorMock.Object, loaderMock.Object);

			vrunner.MigrationAssembly.ShouldBe(Assembly.GetAssembly(typeof(MigrationVersionRunnerUnitTests)));
		}

		[Test]
		public void HandlesNullMigrationList()
		{
			//mock up the dependencies
			var conventionMock = new Mock<IMigrationConventions>(MockBehavior.Loose);
			var processorMock = new Mock<IMigrationProcessor>(MockBehavior.Loose);
			var loaderMock = new Mock<IMigrationLoader>(MockBehavior.Loose);

			//set migrations to return empty list
			var asm = Assembly.GetAssembly(typeof(MigrationVersionRunnerUnitTests));
			loaderMock.Setup(x => x.FindMigrationsIn(asm)).Returns<IEnumerable<Migration>>(null);

			var vrunner = new MigrationVersionRunner(conventionMock.Object, processorMock.Object, loaderMock.Object);

			vrunner.Migrations.Count.ShouldBe(0);

			vrunner.UpgradeToLatest(false);

			loaderMock.VerifyAll();
		}
	}
}
