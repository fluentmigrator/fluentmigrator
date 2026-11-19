#region License
// Copyright (c) 2026, Fluent Migrator Project
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

using FluentMigrator.Infrastructure;
using FluentMigrator.Runner.Conventions;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Infrastructure
{
    /// <summary>
    /// The default <see cref="IDefaultSchemaNameResolver"/>: the schema name convention of the configured
    /// <see cref="IConventionSet"/>, the same one the runner applies to expressions.
    /// </summary>
    public class ConventionSetDefaultSchemaNameResolver : IDefaultSchemaNameResolver
    {
        [CanBeNull]
        private readonly IDefaultSchemaNameConvention _schemaNameConvention;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConventionSetDefaultSchemaNameResolver"/> class.
        /// </summary>
        /// <param name="conventionSet">The convention set whose default schema the schema queries use</param>
        public ConventionSetDefaultSchemaNameResolver([NotNull] IConventionSet conventionSet)
        {
            _schemaNameConvention = conventionSet?.SchemaConvention?.SchemaNameConvention;
        }

        /// <inheritdoc />
        public string GetSchemaName(string schemaName)
        {
            return _schemaNameConvention == null ? schemaName : _schemaNameConvention.GetSchemaName(schemaName);
        }
    }
}
