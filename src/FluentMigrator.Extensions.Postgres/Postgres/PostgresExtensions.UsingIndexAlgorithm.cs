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
        public static ICreateIndexOptionsSyntax UsingBTreeAlgorithm(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.UsingIndexAlgorithm(PostgresIndexAlgorithm.BTree);
            return expression;
        }

        public static ICreateIndexNonKeyColumnSyntax UsingBTreeAlgorithm(this ICreateIndexOnColumnSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.UsingIndexAlgorithm(PostgresIndexAlgorithm.BTree);
            return new CreateIndexExpressionNonKeyBuilder(expression, additionalFeatures);
        }

        public static ICreateIndexOptionsSyntax UsingHashAlgorithm(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.UsingIndexAlgorithm(PostgresIndexAlgorithm.Hash);
            return expression;
        }

        public static ICreateIndexNonKeyColumnSyntax UsingHashAlgorithm(this ICreateIndexOnColumnSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.UsingIndexAlgorithm(PostgresIndexAlgorithm.Hash);
            return new CreateIndexExpressionNonKeyBuilder(expression, additionalFeatures);
        }

        public static ICreateIndexOptionsSyntax UsingGistAlgorithm(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.UsingIndexAlgorithm(PostgresIndexAlgorithm.Gist);
            return expression;
        }

        public static ICreateIndexNonKeyColumnSyntax UsingGistAlgorithm(this ICreateIndexOnColumnSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.UsingIndexAlgorithm(PostgresIndexAlgorithm.Gist);
            return new CreateIndexExpressionNonKeyBuilder(expression, additionalFeatures);
        }

        public static ICreateIndexOptionsSyntax UsingSpgistAlgorithm(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.UsingIndexAlgorithm(PostgresIndexAlgorithm.Spgist);
            return expression;
        }

        public static ICreateIndexNonKeyColumnSyntax UsingSpgistAlgorithm(this ICreateIndexOnColumnSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.UsingIndexAlgorithm(PostgresIndexAlgorithm.Spgist);
            return new CreateIndexExpressionNonKeyBuilder(expression, additionalFeatures);
        }

        public static ICreateIndexOptionsSyntax UsingGinAlgorithm(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.UsingIndexAlgorithm(PostgresIndexAlgorithm.Gin);
            return expression;
        }

        public static ICreateIndexNonKeyColumnSyntax UsingGinAlgorithm(this ICreateIndexOnColumnSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.UsingIndexAlgorithm(PostgresIndexAlgorithm.Gin);
            return new CreateIndexExpressionNonKeyBuilder(expression, additionalFeatures);
        }

        public static ICreateIndexOptionsSyntax UsingBrinAlgorithm(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.UsingIndexAlgorithm(PostgresIndexAlgorithm.Brin);
            return expression;
        }

        public static ICreateIndexNonKeyColumnSyntax UsingBrinAlgorithm(this ICreateIndexOnColumnSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.UsingIndexAlgorithm(PostgresIndexAlgorithm.Brin);
            return new CreateIndexExpressionNonKeyBuilder(expression, additionalFeatures);
        }

        internal static void UsingIndexAlgorithm(this ISupportAdditionalFeatures additionalFeatures, PostgresIndexAlgorithm algorithm)
        {
            if (additionalFeatures == null)
            {
                throw new InvalidOperationException(UnsupportedMethodMessage($"Using{algorithm}Algorithm", nameof(ISupportAdditionalFeatures)));
            }

            var algorithmDefinition = additionalFeatures.GetAdditionalFeature(IndexAlgorithm,  new PostgresIndexAlgorithmDefinition());
            algorithmDefinition.Algorithm = algorithm;
        }
    }
}
