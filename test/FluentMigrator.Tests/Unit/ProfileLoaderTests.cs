using System.Linq;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Moq;
using NUnit.Framework;
using NUnit.Should;
using FluentMigrator.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class ProfileLoaderTests
    {
        [Test]
        public void BlankProfileDoesntLoadProfiles()
        {
            var _runnerContextMock = new Mock<IRunnerContext>();
            var _runnerMock = new Mock<IMigrationRunner>();
            var _conventionsMock = new Mock<IMigrationRunnerConventions>();

            _runnerContextMock.Setup(x => x.Profile).Returns(string.Empty);

            var profileLoader = new ServiceCollection()
                .ConfigureRunner(rb =>
                    rb.WithRunnerContext(_runnerContextMock.Object).WithRunnerConventions(_conventionsMock.Object))
                .AddScoped<ProfileLoader>()
                .WithAllTestMigrations()
                .BuildServiceProvider()
                .GetRequiredService<ProfileLoader>();

            profileLoader.ApplyProfiles(_runnerMock.Object);

            profileLoader.Profiles.Count().ShouldBe(0);
        }
    }
}
