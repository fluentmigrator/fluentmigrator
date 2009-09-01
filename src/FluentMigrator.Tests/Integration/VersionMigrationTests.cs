using FluentMigrator.Runner;
using FluentMigrator.Runner.Versioning;
using Xunit;
using Xunit.Extensions;

namespace FluentMigrator.Tests.Integration
{
    public class VersionMigrationTests
    {
        [Theory]
        [Sqlite]
        [SqlServer]
        public void CanUseVersionInfo(IMigrationProcessor processor)
        {
            var conventions = new MigrationConventions();
            var runner = new MigrationRunner(conventions, processor);

            //ensure table doesn't exist
            if(processor.TableExists(VersionInfo.TABLE_NAME))
                runner.Down(new VersionMigration());

            runner.Up(new VersionMigration());
            Assert.True(processor.TableExists(VersionInfo.TABLE_NAME));
            runner.Down(new VersionMigration());
            Assert.False(processor.TableExists(VersionInfo.TABLE_NAME));
        }
    }
}
