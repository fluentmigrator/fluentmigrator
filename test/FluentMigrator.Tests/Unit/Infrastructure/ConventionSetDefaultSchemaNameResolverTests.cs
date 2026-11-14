#region License
// Copyright (c) 2026, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Infrastructure;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

using Shouldly;

namespace FluentMigrator.Tests.Unit.Infrastructure
{
    [TestFixture]
    [Category("Infrastructure")]
    public class ConventionSetDefaultSchemaNameResolverTests
    {
        [Test]
        public void NoSchemaGivesTheDefaultSchema()
        {
            var resolver = new ConventionSetDefaultSchemaNameResolver(ConventionSets.CreateTestSchemaName(null));

            resolver.GetSchemaName(null).ShouldBe("testdefault");
        }

        [Test]
        public void AnExplicitSchemaWinsOverTheDefaultSchema()
        {
            var resolver = new ConventionSetDefaultSchemaNameResolver(ConventionSets.CreateTestSchemaName(null));

            resolver.GetSchemaName("other").ShouldBe("other");
        }

        [Test]
        public void WithoutADefaultSchemaTheSchemaIsUnchanged()
        {
            var resolver = new ConventionSetDefaultSchemaNameResolver(ConventionSets.CreateNoSchemaName(null));

            resolver.GetSchemaName(null).ShouldBeNull();
            resolver.GetSchemaName("other").ShouldBe("other");
        }

        [Test]
        public void AConventionSetWithoutASchemaConventionLeavesTheSchemaUnchanged()
        {
            var resolver = new ConventionSetDefaultSchemaNameResolver(new ConventionSet());

            resolver.GetSchemaName(null).ShouldBeNull();
            resolver.GetSchemaName("other").ShouldBe("other");
        }

        [Test]
        public void TheRunnerResolvesTheConfiguredConventionSet()
        {
            using var serviceProvider = ServiceCollectionExtensions.CreateServices()
                .AddSingleton<IConventionSet>(new DefaultConventionSet("foo", workingDirectory: null))
                .BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            scope.ServiceProvider.GetRequiredService<IDefaultSchemaNameResolver>().GetSchemaName(null).ShouldBe("foo");
        }
    }
}
