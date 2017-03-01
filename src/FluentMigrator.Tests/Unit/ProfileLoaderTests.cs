﻿using System.Linq;
using System.Reflection;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Moq;
using Xunit;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Tests.Unit
{
	public class ProfileLoaderTests
	{
		[Fact]
		public void BlankProfileDoesntLoadProfiles()
		{
			var _runnerContextMock = new Mock<IRunnerContext>();
			var _runnerMock = new Mock<IMigrationRunner>();
			var _conventionsMock = new Mock<IMigrationConventions>();

            _runnerContextMock.Setup(x => x.Profile).Returns(string.Empty);
			//_runnerContextMock.VerifyGet(x => x.Profile).Returns(string.Empty);
			_runnerMock.SetupGet(x => x.MigrationAssemblies).Returns(new SingleAssembly(typeof(MigrationRunnerTests).Assembly));

			var profileLoader = new ProfileLoader(_runnerContextMock.Object, _runnerMock.Object, _conventionsMock.Object);

			profileLoader.ApplyProfiles();

			profileLoader.Profiles.Count().ShouldBe(0);
		}
	}
}
