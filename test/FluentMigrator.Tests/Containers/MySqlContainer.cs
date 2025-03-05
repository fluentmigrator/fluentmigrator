﻿#region License
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

using Testcontainers.MySql;

namespace FluentMigrator.Tests.Containers;

public class MySqlContainer : ContainerBase
{
    /// <inheritdoc />
    protected override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.MySql;

    /// <inheritdoc />
    protected override int Port => 3306;

    /// <inheritdoc />
    protected override DockerContainer Build() => new MySqlBuilder().WithReuse(true).Build();
}
