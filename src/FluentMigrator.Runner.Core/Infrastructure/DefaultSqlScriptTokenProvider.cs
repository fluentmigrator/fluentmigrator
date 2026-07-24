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
using FluentMigrator.Runner.Conventions;

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Infrastructure
{
    /// <summary>
    /// The default <see cref="ISqlScriptTokenProvider"/> implementation, exposing the
    /// <c>DefaultSchema</c> token derived from the currently configured
    /// <see cref="IConventionSet.SchemaConvention"/>.
    /// </summary>
    public class DefaultSqlScriptTokenProvider : ISqlScriptTokenProvider
    {
        private readonly IConventionSet _conventionSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSqlScriptTokenProvider"/> class.
        /// </summary>
        /// <param name="conventionSet">The convention set used to resolve the default schema name</param>
        public DefaultSqlScriptTokenProvider([NotNull] IConventionSet conventionSet)
        {
            _conventionSet = conventionSet;
        }

        /// <inheritdoc />
        public IDictionary<string, string> GetTokens()
        {
            var tokens = new Dictionary<string, string>();

            var defaultSchema = _conventionSet?.SchemaConvention?.SchemaNameConvention?.GetSchemaName(null);
            if (!string.IsNullOrEmpty(defaultSchema))
            {
                tokens["DefaultSchema"] = defaultSchema;
            }

            return tokens;
        }
    }
}
