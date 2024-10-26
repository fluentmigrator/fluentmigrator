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
using System.Collections.Generic;

using FluentMigrator.Builder.Create.Index;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;

namespace FluentMigrator.Postgres
{
    public static partial class PostgresExtensions
    {
        public static ICreateIndexOptionsSyntax Include(this ICreateIndexOptionsSyntax expression, string columnName)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Include(columnName);
            return expression;
        }

        public static ICreateIndexNonKeyColumnSyntax Include(this ICreateIndexOnColumnSyntax expression, string columnName)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Include(columnName);
            return new CreateIndexExpressionNonKeyBuilder(expression, additionalFeatures);
        }

        internal static void Include(this ISupportAdditionalFeatures additionalFeatures, string columnName)
        {
            if (additionalFeatures == null)
            {
                throw new InvalidOperationException(UnsupportedMethodMessage(nameof(Include), nameof(ISupportAdditionalFeatures)));
            }

            var includes = additionalFeatures.GetAdditionalFeature<IList<PostgresIndexIncludeDefinition>>(IncludesList, () => new List<PostgresIndexIncludeDefinition>());
            includes.Add(new PostgresIndexIncludeDefinition { Name = columnName });
        }
    }
}
