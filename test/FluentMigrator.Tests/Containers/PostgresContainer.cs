#region License
// Copyright (c) 2024, Fluent Migrator Project
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

using DotNet.Testcontainers.Containers;

using Testcontainers.PostgreSql;

namespace FluentMigrator.Tests.Containers;

/// <summary>
/// Container for PostgreSQL with the Anonymizer extension.
/// Uses the registry.gitlab.com/dalibo/postgresql_anonymizer image to enable
/// testing of security labels with the anon extension.
/// </summary>
public class PostgresContainer : ContainerBase
{
    /// <inheritdoc />
    protected override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.Postgres;

    /// <inheritdoc />
    protected override int Port => 5432;

    /// <inheritdoc />
    protected override DockerContainer Build() => new PostgreSqlBuilder()
        .WithImage("registry.gitlab.com/dalibo/postgresql_anonymizer:stable")
        .WithReuse(true)
        .Build();
}
