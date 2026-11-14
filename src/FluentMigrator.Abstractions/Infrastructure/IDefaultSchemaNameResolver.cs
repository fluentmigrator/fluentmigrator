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

using JetBrains.Annotations;

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// Gives the schema queries (<c>Schema.Table(...).Exists()</c>, <c>Schema.Sequence(...).Exists()</c> and the
    /// column, index and constraint checks under <c>Schema.Table(...)</c>) the schema the migration's expressions use.
    /// </summary>
    /// <remarks>
    /// The runner applies the default schema of its convention set to every expression, so
    /// <c>Create.Table("Users")</c> creates <c>Users</c> in that schema. The schema queries run immediately, while the
    /// migration builds its expressions, so the runner can't apply conventions to them; they ask this service instead.
    /// Without it, a query with no schema looks in the connection's default schema, which may not be where the
    /// migration created the object.
    /// </remarks>
    public interface IDefaultSchemaNameResolver
    {
        /// <summary>
        /// Gets the schema to query
        /// </summary>
        /// <param name="schemaName">The schema the migration named, or <c>null</c> when it named none</param>
        /// <returns>The schema the migration's expressions would use for <paramref name="schemaName"/></returns>
        [CanBeNull]
        string GetSchemaName([CanBeNull] string schemaName);
    }
}
