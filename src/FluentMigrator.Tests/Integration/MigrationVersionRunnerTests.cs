using FluentMigrator.Runner;
using Xunit;
using Xunit.Extensions;

namespace FluentMigrator.Tests.Integration
{
    public class MigrationVersionRunnerTests
    {
        [Theory]
        [Sqlite]
        [SqlServer]
        public void CanLoadMigrations(IMigrationProcessor processor)
        {   
            var conventions = new MigrationConventions();
            
            var runner = new MigrationVersionRunner(conventions, processor, typeof(MigrationVersionRunnerTests));

            Assert.NotNull(runner.Migrations);
        }

        [Theory]
        [Sqlite]
        [SqlServer]
        public void CanLoadVersion(IMigrationProcessor processor)
        {
            var conventions = new MigrationConventions();
            
            var runner = new MigrationVersionRunner(conventions, processor, typeof(MigrationVersionRunnerTests));

            Assert.NotNull(runner.Version);            
        }

        [Theory]
        [Sqlite]
        [SqlServer]
        public void CanRunMigration(IMigrationProcessor processor)
        {
            var conventions = new MigrationConventions();

            var runner = new MigrationVersionRunner(conventions, processor, typeof(MigrationVersionRunnerTests));

            runner.UpgradeToVersion(2, false);

            Assert.Equal<long>((long)2, runner.Version.CurrentVersion);

            //now step down to 0
            long last = 0;
            runner.StepDown(runner.CurrentVersion, 0, out last);

            Assert.Equal<long>((long)0, runner.CurrentVersion);
        }

        [Theory]
        [Sqlite]
        [SqlServer]
        public void CanUpdgradeToLatest(IMigrationProcessor processor)
        {
            var conventions = new MigrationConventions();

            var runner = new MigrationVersionRunner(conventions, processor, typeof(MigrationVersionRunnerTests));

            runner.UpgradeToLatest(false);

            Assert.True(true); //made it this far..

            //now step down to 0
            long last = 0;
            runner.StepDown(runner.CurrentVersion, 0, out last);

            Assert.Equal<long>((long)0, runner.CurrentVersion);
        }
    }
}
