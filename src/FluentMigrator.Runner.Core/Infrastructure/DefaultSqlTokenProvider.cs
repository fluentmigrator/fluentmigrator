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

using FluentMigrator.Infrastructure;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Infrastructure
{
    /// <summary>
    /// The default <see cref="ISqlTokenProvider"/> implementation, exposing the <c>DefaultSchema</c> token.
    /// </summary>
    /// <remarks>
    /// <c>DefaultSchema</c> is <see cref="IDefaultSchemaNameResolver.GetSchemaName"/> for no schema: the schema that
    /// <c>Create.Table("X")</c> creates <c>X</c> in and that <c>Schema.Table("X").Exists()</c> checks. Taking it from the
    /// resolver, rather than from the convention set directly, keeps the token and the schema checks identical even
    /// when a custom <see cref="IDefaultSchemaNameResolver"/> is registered.
    /// </remarks>
    public class DefaultSqlTokenProvider : ISqlTokenProvider
    {
        private readonly IDefaultSchemaNameResolver _schemaNameResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSqlTokenProvider"/> class.
        /// </summary>
        /// <param name="schemaNameResolver">The resolver that gives the default schema name</param>
        public DefaultSqlTokenProvider([NotNull] IDefaultSchemaNameResolver schemaNameResolver)
        {
            _schemaNameResolver = schemaNameResolver;
        }

        /// <inheritdoc />
        public IDictionary<string, string> GetTokens()
        {
            var tokens = new Dictionary<string, string>();

            var defaultSchema = _schemaNameResolver?.GetSchemaName(null);
            if (!string.IsNullOrEmpty(defaultSchema))
            {
                tokens["DefaultSchema"] = defaultSchema;
            }

            return tokens;
        }
    }
}
