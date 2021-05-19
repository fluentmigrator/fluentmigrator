#region License
// Copyright (c) 2020, FluentMigrator Project
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

using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;

namespace FluentMigrator.Postgres
{
    public static partial class PostgresExtensions
    {
        public const string IndexTablespace = "PostgresTablespace";

        /// <summary>
        /// The tablespace in which to create the index. If not specified, default_tablespace is consulted, or temp_tablespaces for indexes on temporary tables.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="tablespace">The tablespace</param>
        /// <returns>The next step</returns>
        public static ICreateIndexOptionsSyntax Tablespace(this ICreateIndexOptionsSyntax expression, string tablespace)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.SetAdditionalFeature(IndexTablespace, tablespace);
            return expression;
        }
    }
}
