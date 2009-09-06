using FluentMigrator.Runner.Versioning;
using NUnit.Framework;
using NUnit.Should;

namespace FluentMigrator.Tests.Unit.Versioning
{
    [TestFixture]
    public class VersionInfoTests
    {
        [Test]
        public void CanAddAppliedMigration()
        {
            var versionInfo = new VersionInfo();
            versionInfo.AddAppliedMigration(200909060953);
            versionInfo.HasAppliedMigration(200909060953).ShouldBeTrue();
        }

        [Test]
        public void CanGetLatestMigration()
        {
            var versionInfo = new VersionInfo();
            versionInfo.AddAppliedMigration(200909060953);
            versionInfo.AddAppliedMigration(200909060935);
            versionInfo.Latest().ShouldBe(200909060953);
        }
    }
}
