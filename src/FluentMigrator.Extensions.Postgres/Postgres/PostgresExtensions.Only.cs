#region License
// Copyright (c) 2020, Fluent Migrator Project
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

using System;

using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;

namespace FluentMigrator.Postgres
{
    public static partial class PostgresExtensions
    {
        /// <summary>
        /// Indicates not to recurse creating indexes on partitions, if the table is partitioned.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns>The next step</returns>
        public static ICreateIndexOptionsSyntax AsOnly(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.AsOnly(true);
            return expression;
        }

        /// <summary>
        /// Indicates not to recurse creating indexes on partitions, if the table is partitioned.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="isOnly">if should or shouldn't be only</param>
        /// <returns>The next step</returns>
        public static ICreateIndexOptionsSyntax AsOnly(this ICreateIndexOptionsSyntax expression, bool isOnly)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.AsOnly(isOnly);
            return expression;
        }

        internal static void AsOnly(this ISupportAdditionalFeatures additionalFeatures, bool isOnly)
        {
            if (additionalFeatures == null)
            {
                throw new InvalidOperationException(UnsupportedMethodMessage(nameof(Include), nameof(ISupportAdditionalFeatures)));
            }

            var asOnly = additionalFeatures.GetAdditionalFeature(Only, () => new PostgresIndexOnlyDefinition());
            asOnly.IsOnly = isOnly;
        }
    }
}
