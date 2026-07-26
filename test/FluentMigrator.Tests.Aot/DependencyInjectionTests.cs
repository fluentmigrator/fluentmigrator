#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using FluentMigrator.Runner;
using FluentMigrator.Runner.Infrastructure;
using FluentMigrator.Validation;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Tests.Aot;

/// <summary>
/// AOT smoke tests for dependency injection.
/// Verifies that the FluentMigrator DI container can build and resolve core services
/// in a trimmed/AOT environment using <c>AddFluentMigratorSlim()</c>.
/// </summary>
public class DependencyInjectionTests
{
    [Test]
    [DisplayName("DI container builds and resolves core services in AOT")]
    public async Task CanBuildAndResolveCoreServices()
    {
        var services = new ServiceCollection();
        services.AddFluentMigratorSlim()
            .ConfigureRunner(rb => rb.AddSQLite());

        var provider = services.BuildServiceProvider();

        await Assert.That(provider.GetService<IMigrationExpressionValidator>()).IsNotNull();
        await Assert.That(provider.GetService<IMigrationRunnerConventions>()).IsNotNull();
        await Assert.That(provider.GetService<IMigrationRunnerTagConventions>()).IsNotNull();
    }
}
