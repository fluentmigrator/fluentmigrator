using System.Linq;
using FluentMigrator.Runner.Versioning;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Versioning
{
	[TestFixture]
	public class VersionInfoTests
	{
		private VersionInfo _versionInfo;

		[SetUp]
		public void SetUp()
		{
			_versionInfo = new VersionInfo();			
		}

		[Test]
		public void CanAddAppliedMigration()
		{
			_versionInfo.AddAppliedMigration(200909060953);
			_versionInfo.HasAppliedMigration(200909060953).ShouldBeTrue();
		}

		[Test]
		public void CanGetLatestMigration()
		{
			_versionInfo.AddAppliedMigration(200909060953);
			_versionInfo.AddAppliedMigration(200909060935);
			_versionInfo.Latest().ShouldBe(200909060953);
		}

		[Test]
		public void CanGetAppliedMigrationsLatestFirst()
		{
			_versionInfo.AddAppliedMigration(200909060953);
			_versionInfo.AddAppliedMigration(200909060935);
			var applied = _versionInfo.AppliedMigrations().ToList();
			applied[0].ShouldBe(200909060953);
			applied[1].ShouldBe(200909060935);
		}
	}
}
