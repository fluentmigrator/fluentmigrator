#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
using System.Linq.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Integration
{
	[TestFixture, Ignore("Needs to be refactored from changes to TaskExecutor")]
	public class TaskExecutorTests : IntegrationTestBase
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

			var processor = new Mock<IMigrationProcessor>();
			var dataSet = new DataSet();
			dataSet.Tables.Add(new DataTable());
			processor.Setup(x => x.ReadTableData(It.IsAny<string>())).Returns(dataSet);

			var runnerContext = new Mock<IRunnerContext>();
			runnerContext.SetupGet(x => x.Database).Returns("sqlserver");
			runnerContext.SetupGet(x => x.Connection).Returns(IntegrationTestOptions.SqlServer.ConnectionString);
			runnerContext.SetupGet(x => x.Task).Returns(task);
			runnerContext.SetupGet(x => x.Version).Returns(version);
			runnerContext.SetupGet(x => x.Steps).Returns(steps);
			runnerContext.SetupGet(x => x.Target).Returns(GetType().Assembly.Location);
			runnerContext.SetupGet(x => x.Processor).Returns(processor.Object);
			runnerContext.SetupGet(x => x.Namespace).Returns("FluentMigrator.Tests.Integration.Migrations.Interleaved.Pass3");

			new TaskExecutor(runnerContext.Object).Execute();

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
