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

using System;
using System.Collections.Generic;

using FluentMigrator.Expressions;

using JetBrains.Annotations;


namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// The default implementation of the <see cref="IMigrationContext"/>
    /// </summary>
    public class MigrationContext : IMigrationContext
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationContext"/> class.
        /// </summary>
        /// <param name="querySchema">The provider used to query the database</param>
        /// <param name="serviceProvider">The service provider</param>
        /// <param name="connection">The database connection</param>
        public MigrationContext(
            [NotNull] IQuerySchema querySchema,
            [NotNull] IServiceProvider serviceProvider,
            string connection)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            QuerySchema = querySchema;
            Connection = connection;
            ServiceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public virtual ICollection<IMigrationExpression> Expressions { get; set; } = new List<IMigrationExpression>();

        /// <inheritdoc />
        public virtual IQuerySchema QuerySchema { get; set; }

        /// <inheritdoc />
        public string Connection { get; set; }

        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; }
    }
}
