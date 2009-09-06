using System;
using System.Linq.Expressions;
using FluentMigrator.Console;
using FluentMigrator.Runner;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit
{
	[TestFixture]
	public class TaskExecutorTests
	{
		private Mock<IMigrationVersionRunner> _migrationVersionRunner;

		[SetUp]
		public void SetUp()
		{
			_migrationVersionRunner = new Mock<IMigrationVersionRunner>();
		}

		private void verify(Expression<Action<IMigrationVersionRunner>> func, string task, long version, int steps)
		{
			_migrationVersionRunner.Setup(func).Verifiable();

			new TaskExecutor(_migrationVersionRunner.Object, version, steps).Execute(task);

			_migrationVersionRunner.VerifyAll();
		}

		[Test]
		public void ShouldCallMigrateUpByDefault()
		{
			verify(x => x.MigrateUp(), null, 0, 0);
			verify(x => x.MigrateUp(), "", 0, 0);
		}

		[Test]
		public void ShouldCallMigrateUpIfSpecified()
		{
			verify(x => x.MigrateUp(), "migrate", 0, 0);
			verify(x => x.MigrateUp(), "migrate:up", 0, 0);
		}

		[Test]
		public void ShouldCallMigrateUpWithVersionIfSpecified()
		{
			verify(x => x.MigrateUp(It.Is<long>(c => c == 1)), "migrate", 1, 0);
			verify(x => x.MigrateUp(It.Is<long>(c => c == 1)), "migrate:up", 1, 0);
		}

		[Test]
		public void ShouldCallRollbackIfSpecified()
		{
			verify(x => x.Rollback(It.Is<int>(c => c == 2)), "rollback", 0, 2);
		}

		[Test]
		public void ShouldCallRollbackIfSpecifiedAndDefaultTo1Step()
		{
			verify(x => x.Rollback(It.Is<int>(c => c == 1)), "rollback", 0, 0);
		}

		[Test]
		public void ShouldCallMigrateDownIfSpecified()
		{
			verify(x => x.MigrateDown(It.Is<long>(c => c == 20)), "migrate:down", 20, 0);			
		}
	}
}
