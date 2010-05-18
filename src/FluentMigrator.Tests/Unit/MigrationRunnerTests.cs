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
using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Tests.Integration.Migrations;
using Moq;
using NUnit.Framework;

namespace FluentMigrator.Tests.Unit
{
	[TestFixture]
	public class MigrationRunnerTests
	{
		private MigrationRunner _runner;
		private Mock<IAnnouncer> _announcer;
		private Mock<IMigrationProcessor> _processor;
		private Mock<IStopWatch> _stopWatch;

		[SetUp]
		public void SetUp()
		{
			var options = new ProcessorOptions
							{
								PreviewOnly = true
							};
			_processor = new Mock<IMigrationProcessor>();
			_processor.SetupGet(x => x.Options).Returns(options);

			_announcer = new Mock<IAnnouncer>();
			_stopWatch = new Mock<IStopWatch>();
			_runner = new MigrationRunner(new MigrationConventions(), _processor.Object, _announcer.Object, _stopWatch.Object);
		}

		[Test]
		public void CanAnnounceUp()
		{
			_announcer.Setup(x => x.Heading(It.IsRegex(containsAll("Test", "migrating"))));
			_runner.Up(new TestMigration());
			_announcer.VerifyAll();
		}

		[Test]
		public void CanAnnounceUpFinish()
		{
			_announcer.Setup(x => x.Say(It.IsRegex(containsAll("Test", "migrated"))));
			_runner.Up(new TestMigration());
			_announcer.VerifyAll();
		}

		[Test]
		public void CanAnnounceDown()
		{
			_announcer.Setup(x => x.Heading(It.IsRegex(containsAll("Test", "reverting"))));
			_runner.Down(new TestMigration());
			_announcer.VerifyAll();
		}

		[Test]
		public void CanAnnounceDownFinish()
		{
			_announcer.Setup(x => x.Say(It.IsRegex(containsAll("Test", "reverted"))));
			_runner.Down(new TestMigration());
			_announcer.VerifyAll();
		}

		[Test]
		public void CanAnnounceUpElapsedTime()
		{
			var ts = new TimeSpan(0, 0, 0, 1, 3);
			_announcer.Setup(x => x.ElapsedTime(It.Is<TimeSpan>(y => y == ts)));

			_stopWatch.Setup(x => x.ElapsedTime()).Returns(ts);

			_runner.Up(new TestMigration());

			_announcer.VerifyAll();
		}

		[Test]
		public void CanAnnounceDownElapsedTime()
		{
			var ts = new TimeSpan(0, 0, 0, 1, 3);
			_announcer.Setup(x => x.ElapsedTime(It.Is<TimeSpan>(y => y == ts)));

			_stopWatch.Setup(x => x.ElapsedTime()).Returns(ts);

			_runner.Down(new TestMigration());

			_announcer.VerifyAll();
		}

		[Test]
		public void CanReportExceptions()
		{
			_processor.Setup(x => x.Process(It.IsAny<CreateTableExpression>())).Throws(new Exception("Oops"));

			_announcer.Setup(x => x.Error(It.IsRegex(containsAll("Oops"))));

			try
			{
				_runner.Up(new TestMigration());
			}
			catch (Exception)
			{
			}

			_announcer.VerifyAll();
		}

		[Test]
		public void CanSayExpression()
		{
			_announcer.Setup(x => x.Say(It.IsRegex(containsAll("CreateTable"))));

			_stopWatch.Setup(x => x.ElapsedTime()).Returns(new TimeSpan(0, 0, 0, 1, 3));

			_runner.Up(new TestMigration());

			_announcer.VerifyAll();
		}

		[Test]
		public void CanTimeExpression()
		{
			var ts = new TimeSpan(0, 0, 0, 1, 3);
			_announcer.Setup(x => x.ElapsedTime(It.Is<TimeSpan>(y => y == ts)));

			_stopWatch.Setup(x => x.ElapsedTime()).Returns(ts);

			_runner.Up(new TestMigration());

			_announcer.VerifyAll();
		}

		private string containsAll(params string[] words)
		{
			return ".*?" + string.Join(".*?", words) + ".*?";
		}
	}
}
