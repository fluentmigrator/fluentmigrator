#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
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

using System;
using System.Collections.Generic;

using FluentMigrator.Expressions;

using JetBrains.Annotations;

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// The context of a migration while collecting up/down expressions
    /// </summary>
    public interface IMigrationContext
    {
        /// <summary>
        /// Gets the service provider used to create this migration context
        /// </summary>
        [NotNull]
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets or sets the collection of expressions
        /// </summary>
        ICollection<IMigrationExpression> Expressions { get; set; }

        /// <summary>
        /// Gets the <see cref="IQuerySchema"/> to access the database
        /// </summary>
        IQuerySchema QuerySchema { get; }

        /// <summary>
        /// Gets or sets the collection of migration assemblies
        /// </summary>
        [Obsolete]
        [CanBeNull]
        IAssemblyCollection MigrationAssemblies { get; set; }

        /// <summary>
        /// Gets or sets the arbitrary application context passed to the task runner
        /// </summary>
        [Obsolete("Use dependency injection to access 'application state'.")]
        object ApplicationContext { get; set; }

        /// <summary>
        /// Gets or sets the connection string
        /// </summary>
        string Connection { get; set; }
    }
}
