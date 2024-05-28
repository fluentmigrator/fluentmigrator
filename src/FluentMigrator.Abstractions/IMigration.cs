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

using FluentMigrator.Builders.Alter;
using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Builders.Rename;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Infrastructure;

namespace FluentMigrator
{
    /// <summary>
    /// The base interface for all migrations
    /// </summary>
    public interface IMigration
    {
        /// <summary>
        /// Gets the connection string passed to the task runner
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Collects all Up migration expressions in the <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The context to use while collecting the Up migration expressions</param>
        void GetUpExpressions(IMigrationContext context);

        /// <summary>
        /// Collects all Down migration expressions in the <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The context to use while collecting the Down migration expressions</param>
        void GetDownExpressions(IMigrationContext context);

        /// <summary>
        /// Gets the starting point for alterations
        /// </summary>
        IAlterExpressionRoot Alter { get; }

        /// <summary>
        /// Gets the starting point for creating database objects
        /// </summary>
        ICreateExpressionRoot Create { get; }

        /// <summary>
        /// Gets the starting point for renaming database objects
        /// </summary>
        IRenameExpressionRoot Rename { get; }

        /// <summary>
        /// Gets the starting point for data insertion
        /// </summary>
        IInsertExpressionRoot Insert { get; }

        /// <summary>
        /// Gets the starting point for schema-rooted expressions
        /// </summary>
        ISchemaExpressionRoot Schema { get; }
    }
}
