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

namespace FluentMigrator.Builders.Schema
{
    internal static class SchemaQueryContextExtensions
    {
        /// <summary>
        /// The schema the migration's expressions use for objects in <paramref name="schemaName"/>, so that a query
        /// looks where <c>Create.Table</c> or <c>Create.Sequence</c> puts them. Without an
        /// <see cref="IDefaultSchemaNameResolver"/>, the schema as given.
        /// </summary>
        /// <remarks>
        /// Only for objects inside a schema. <c>Create.Schema(...)</c> doesn't go through the schema convention, so
        /// neither does <c>Schema.Schema(...).Exists()</c>.
        /// </remarks>
        internal static string GetObjectSchemaName(this IMigrationContext context, string schemaName)
        {
            var resolver = context.ServiceProvider?.GetService(typeof(IDefaultSchemaNameResolver)) as IDefaultSchemaNameResolver;
            return resolver == null ? schemaName : resolver.GetSchemaName(schemaName);
        }
    }
}
