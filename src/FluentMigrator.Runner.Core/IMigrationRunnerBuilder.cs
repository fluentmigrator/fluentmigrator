#region License
// Copyright (c) 2018, FluentMigrator Project
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

using System.ComponentModel;

using FluentMigrator.Runner.Initialization;

using JetBrains.Annotations;

using Microsoft.Extensions.DependencyInjection;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// An interface for configuring migration runner services
    /// </summary>
    public interface IMigrationRunnerBuilder
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> where the migration runner services are configured
        /// </summary>
        [NotNull]
        IServiceCollection Services { get; }

        /// <summary>
        /// Gets the dangling assembly source item (when no usage was specified)
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [CanBeNull]
        IAssemblySourceItem DanglingAssemblySourceItem { get; set; }
    }
}
