#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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
    /// A set of conventions to be applied to expressions
    /// </summary>
    public interface IConventionSet
    {
        /// <summary>
        /// Gets the root path convention to be applied to <see cref="IFileSystemExpression"/> implementations
        /// </summary>
        IRootPathConvention RootPathConvention { get; }

        /// <summary>
        /// Gets the default schema name convention to be applied to <see cref="ISchemaExpression"/> implementations
        /// </summary>
        /// <remarks>
        /// This class cannot be overridden. The <see cref="IDefaultSchemaNameConvention"/>
        /// must be implemented/provided instead.
        /// </remarks>
        DefaultSchemaConvention SchemaConvention { get; }

        /// <summary>
        /// Gets the conventions to be applied to <see cref="IColumnsExpression"/> implementations
        /// </summary>
        IList<IColumnsConvention> ColumnsConventions { get; }

        /// <summary>
        /// Gets the conventions to be applied to <see cref="IConstraintExpression"/> implementations
        /// </summary>
        IList<IConstraintConvention> ConstraintConventions { get; }

        /// <summary>
        /// Gets the conventions to be applied to <see cref="IForeignKeyExpression"/> implementations
        /// </summary>
        IList<IForeignKeyConvention> ForeignKeyConventions { get; }

        /// <summary>
        /// Gets the conventions to be applied to <see cref="IIndexExpression"/> implementations
        /// </summary>
        IList<IIndexConvention> IndexConventions { get; }

        /// <summary>
        /// Gets the conventions to be applied to <see cref="ISequenceExpression"/> implementations
        /// </summary>
        IList<ISequenceConvention> SequenceConventions { get; }

        /// <summary>
        /// Gets the conventions to be applied to <see cref="IAutoNameExpression"/> implementations
        /// </summary>
        IList<IAutoNameConvention> AutoNameConventions { get; }
    }
}
