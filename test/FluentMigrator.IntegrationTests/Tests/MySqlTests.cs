using FluentAssertions;

using FluentMigrator.IntegrationTests.Fixtures.MySql5;
using FluentMigrator.Runner;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace FluentMigrator.IntegrationTests.Tests
{
    [Collection("MySql")]
    public class MySqlTests : IClassFixture<TestFixture>
    {
        private readonly IMigrationRunner _runner;

        public MySqlTests(TestFixture fixture)
        {
            _runner = fixture.Server.Services.GetService<IMigrationRunner>();
        }

        [Trait("MySql", "5.6")]
        [Fact]
        public void ShouldApplyAllMigrations()
        {
            _runner.MigrateUp();

            _runner.HasMigrationsToApplyUp().Should().BeFalse();
            _runner.Processor.TableExists(null, "TestTable").Should().BeTrue();
            _runner.Processor.TableExists(null, "TestTable2").Should().BeTrue();
        }
    }
}
