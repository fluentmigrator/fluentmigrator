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

using System.Threading;
using System.Threading.Tasks;

using DotNet.Testcontainers.Containers;

namespace FluentMigrator.Tests.Containers;

public abstract class ContainerBase
{
    /// <summary>
    /// Server options
    /// </summary>
    protected abstract IntegrationTestOptions.DatabaseServerOptions ServerOptions { get; }

    /// <summary>
    /// Default container database port
    /// </summary>
    protected abstract int Port  { get; }

    /// <summary>
    /// Builds the test-container container
    /// </summary>
    protected abstract DockerContainer Build();

    public async Task Start(CancellationToken cancellationToken = default)
    {
        if (!ServerOptions.IsEnabled || !ServerOptions.ContainerEnabled)
        {
            return;
        }

        // Build containers
        var container = Build();

        // Start it
        await container.StartAsync(cancellationToken);

        // Get the external port
        var publicPort = container.GetMappedPublicPort(Port);
        ServerOptions.ConnectionString = ServerOptions.ConnectionString.Replace(Port.ToString(), publicPort.ToString());
    }
}
