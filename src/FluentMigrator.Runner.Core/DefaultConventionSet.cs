#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System.Collections.Generic;

using FluentMigrator.Runner.Conventions;

using JetBrains.Annotations;

namespace FluentMigrator.Runner
{
    /// <summary>
    /// Represents the default set of conventions used by FluentMigrator during migration operations.
    /// </summary>
    /// <remarks>
    /// This class provides a predefined set of conventions for schema, constraints, foreign keys, indexes, 
    /// sequences, and other migration-related elements. It implements the <see cref="FluentMigrator.Runner.Conventions.IConventionSet"/> 
    /// interface and is designed to simplify the configuration of migrations by applying consistent rules and behaviors.
    /// </remarks>
    public class DefaultConventionSet : IConventionSet
    {
        // ReSharper disable once UnusedMember.Global
        /// <summary>
        /// Initializes a new instance of the <see cref="FluentMigrator.Runner.DefaultConventionSet"/> class
        /// with default conventions for schema, constraints, foreign keys, indexes, sequences, and more.
        /// </summary>
        /// <remarks>
        /// This constructor sets up a default set of conventions to be used during migration operations.
        /// It delegates to the parameterized constructor with default values for schema name and working directory.
        /// </remarks>
        public DefaultConventionSet()
            : this(defaultSchemaName: null, workingDirectory: null)
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultConventionSet"/> class with the specified default schema name and working directory.
        /// </summary>
        /// <param name="defaultSchemaName">
        /// The default schema name to be used in conventions. Can be <c>null</c>.
        /// </param>
        /// <param name="workingDirectory">
        /// The working directory to be used for path-related conventions. Can be <c>null</c>.
        /// </param>
        public DefaultConventionSet([CanBeNull] string defaultSchemaName, [CanBeNull] string workingDirectory)
        {
            var schemaConvention =
                new DefaultSchemaConvention(new DefaultSchemaNameConvention(defaultSchemaName));

            ColumnsConventions = new List<IColumnsConvention>()
            {
                new DefaultPrimaryKeyNameConvention(),
            };

            ConstraintConventions = new List<IConstraintConvention>()
            {
                new DefaultConstraintNameConvention(),
                schemaConvention,
            };

            ForeignKeyConventions = new List<IForeignKeyConvention>()
            {
                new DefaultForeignKeyNameConvention(),
                schemaConvention,
            };

            IndexConventions = new List<IIndexConvention>()
            {
                new DefaultIndexNameConvention(),
                schemaConvention,
            };

            SequenceConventions = new List<ISequenceConvention>()
            {
                schemaConvention,
            };

            AutoNameConventions = new List<IAutoNameConvention>()
            {
                new DefaultAutoNameConvention(),
            };

            SchemaConvention = schemaConvention;
            RootPathConvention = new DefaultRootPathConvention(workingDirectory);
        }

        /// <inheritdoc />
        public IRootPathConvention RootPathConvention { get; }

        /// <inheritdoc />
        public DefaultSchemaConvention SchemaConvention { get; }

        /// <inheritdoc />
        public IList<IColumnsConvention> ColumnsConventions { get; }

        /// <inheritdoc />
        public IList<IConstraintConvention> ConstraintConventions { get; }

        /// <inheritdoc />
        public IList<IForeignKeyConvention> ForeignKeyConventions { get; }

        /// <inheritdoc />
        public IList<IIndexConvention> IndexConventions { get; }

        /// <inheritdoc />
        public IList<ISequenceConvention> SequenceConventions { get; }

        /// <inheritdoc />
        public IList<IAutoNameConvention> AutoNameConventions { get; }
    }
}
