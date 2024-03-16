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

using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Conventions
{
    /// <summary>
    /// A convenience (empty) implementation of <see cref="IConventionSet"/>
    /// </summary>
    public class ConventionSet : IConventionSet
    {
        /// <summary>
        /// Gets or sets the root path convention to be applied to <see cref="IFileSystemExpression"/> implementations
        /// </summary>
        public IRootPathConvention RootPathConvention { get; set; }

        /// <summary>
        /// Gets or sets the default schema name convention to be applied to <see cref="ISchemaExpression"/> implementations
        /// </summary>
        /// <remarks>
        /// This class cannot be overridden. The <see cref="IDefaultSchemaNameConvention"/>
        /// must be implemented/provided instead.
        /// </remarks>
        public DefaultSchemaConvention SchemaConvention { get; set; }

        /// <inheritdoc />
        public IList<IColumnsConvention> ColumnsConventions { get; } = new List<IColumnsConvention>();

        /// <inheritdoc />
        public IList<IConstraintConvention> ConstraintConventions { get; } = new List<IConstraintConvention>();

        /// <inheritdoc />
        public IList<IForeignKeyConvention> ForeignKeyConventions { get; } = new List<IForeignKeyConvention>();

        /// <inheritdoc />
        public IList<IIndexConvention> IndexConventions { get; } = new List<IIndexConvention>();

        /// <inheritdoc />
        public IList<ISequenceConvention> SequenceConventions { get; } = new List<ISequenceConvention>();

        /// <inheritdoc />
        public IList<IAutoNameConvention> AutoNameConventions { get; } = new List<IAutoNameConvention>();
    }
}
