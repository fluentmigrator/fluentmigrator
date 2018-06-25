#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

using System.Linq;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Tests.Integration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit
{
    [TestFixture]
    public class ProfileLoaderTests
    {
        [Test]
        public void BlankProfileDoesntLoadProfiles()
        {
            var runnerMock = new Mock<IMigrationRunner>();

            var profileLoader = (ProfileLoader)ServiceCollectionExtensions.CreateServices()
                .Configure<RunnerOptions>(opt => opt.Profile = string.Empty)
                .WithMigrationsIn(typeof(IntegrationTestBase).Namespace, true)
                .BuildServiceProvider()
                .GetRequiredService<IProfileLoader>();

            profileLoader.ApplyProfiles(runnerMock.Object);

            profileLoader.Profiles.Count().ShouldBe(0);
        }
    }
}
