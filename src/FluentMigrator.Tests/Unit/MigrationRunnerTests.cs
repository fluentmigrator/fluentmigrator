using System;
using FluentMigrator.Expressions;
using FluentMigrator.Runner;
using FluentMigrator.Tests.Integration.Migrations;
using NUnit.Framework;
using Moq;

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
			_announcer = new Mock<IAnnouncer>();
			_processor = new Mock<IMigrationProcessor>();
			_stopWatch = new Mock<IStopWatch>();
			_runner = new MigrationRunner(new MigrationConventions(), _processor.Object, _announcer.Object, _stopWatch.Object);
		}

		[Test]
		public void CanAnnounceUp()
		{
			_announcer.Setup(x => x.Announce(It.IsRegex(containsAll("Test", "migrating"))));
			_runner.Up(new TestMigration());
			_announcer.VerifyAll();
		}

		[Test]
		public void CanAnnounceUpFinish()
		{
			_announcer.Setup(x => x.Announce(It.IsRegex(containsAll("Test", "migrated"))));
			_runner.Up(new TestMigration());
			_announcer.VerifyAll();
		}

		[Test]
		public void CanAnnounceDown()
		{
			_announcer.Setup(x => x.Announce(It.IsRegex(containsAll("Test", "reverting"))));
			_runner.Down(new TestMigration());
			_announcer.VerifyAll();
		}

		[Test]
		public void CanAnnounceDownFinish()
		{
			_announcer.Setup(x => x.Announce(It.IsRegex(containsAll("Test", "reverted"))));
			_runner.Down(new TestMigration());
			_announcer.VerifyAll();
		}

		[Test]
		public void CanAnnounceUpElapsedTime()
		{
			_announcer.Setup(x => x.Announce(It.IsRegex(containsAll("1.003s"))));

			_stopWatch.Setup(x => x.ElapsedTime()).Returns(new TimeSpan(0, 0, 0, 1, 3));

			_runner.Up(new TestMigration());

			_announcer.VerifyAll();
		}

		[Test]
		public void CanAnnounceDownElapsedTime()
		{
			_announcer.Setup(x => x.Announce(It.IsRegex(containsAll("1.003s"))));

			_stopWatch.Setup(x => x.ElapsedTime()).Returns(new TimeSpan(0, 0, 0, 1, 3));

			_runner.Down(new TestMigration());

			_announcer.VerifyAll();
		}

		[Test]
		public void CanReportExceptions()
		{
			_processor.Setup(x => x.Process(It.IsAny<CreateTableExpression>())).Throws(new Exception("Oops"));

			_announcer.Setup(x => x.Say(It.IsRegex(containsAll("Oops"))));

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
			_announcer.Setup(x => x.SaySubItem(It.IsRegex(containsAll("1.003s"))));

			_stopWatch.Setup(x => x.ElapsedTime()).Returns(new TimeSpan(0, 0, 0, 1, 3));

			_runner.Up(new TestMigration());

			_announcer.VerifyAll();
		}

		private string containsAll(params string[] words)
		{
			return ".*?" + string.Join(".*?", words) + ".*?";
		}
	}
}
