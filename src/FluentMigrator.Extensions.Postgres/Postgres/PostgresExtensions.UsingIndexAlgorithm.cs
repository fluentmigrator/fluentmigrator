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

using FluentMigrator.Builder.Create.Index;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;

namespace FluentMigrator.Postgres
{
    public static partial class PostgresExtensions
    {
        public static ICreateBTreeIndexOptionsSyntax UsingBTree(this ICreateIndexOptionsSyntax expression)
        {
            if (expression is ICreateIndexMethodOptionsSyntax)
            {
                throw new InvalidOperationException("Only can have one index method.");
            }

            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Algorithm.BTree);
            return new CreateBTreeIndexOptionsSyntax(expression);
        }

        public static ICreateHashIndexOptionSyntax UsingHash(this ICreateIndexOptionsSyntax expression)
        {
            if (expression is ICreateIndexMethodOptionsSyntax)
            {
                throw new InvalidOperationException("Only can have one index method.");
            }

            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Algorithm.Hash);
            return new CreateHashIndexOptionSyntax(expression);
        }

        public static ICreateGiSTIndexOptionsSyntax UsingGist(this ICreateIndexOptionsSyntax expression)
        {
            if (expression is ICreateIndexMethodOptionsSyntax)
            {
                throw new InvalidOperationException("Only can have one index method.");
            }

            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Algorithm.Gist);
            return new CreateGistIndexOptionsSyntax(expression);
        }

        public static ICreateSpgistIndexOptionsSyntax UsingSpgist(this ICreateIndexOptionsSyntax expression)
        {
            if (expression is ICreateIndexMethodOptionsSyntax)
            {
                throw new InvalidOperationException("Only can have one index method.");
            }

            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Algorithm.Spgist);
            return new CreateSpgistIndexOptionsSyntax(expression);
        }

        public static ICreateGinIndexOptionsSyntax UsingGin(this ICreateIndexOptionsSyntax expression)
        {
            if (expression is ICreateIndexMethodOptionsSyntax)
            {
                throw new InvalidOperationException("Only can have one index method.");
            }

            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Algorithm.Gin);
            return new CreateGinIndexOptionsSyntax(expression);
        }

        public static ICreateBrinIndexOptionsSyntax UsingBrin(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Algorithm.Brin);
            return new CreateBrinIndexOptionsSyntax(expression);
        }

        public static ICreateIndexMethodOptionsSyntax Using(this ICreateIndexOptionsSyntax expression, Algorithm algorithm)
        {
            if (expression is ICreateIndexMethodOptionsSyntax)
            {
                throw new InvalidOperationException("Only can have one index method.");
            }

            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(algorithm);
            switch (algorithm)
            {
                case Algorithm.BTree:
                    return new CreateBTreeIndexOptionsSyntax(expression);
                case Algorithm.Hash:
                    return new CreateHashIndexOptionSyntax(expression);
                case Algorithm.Gist:
                    return new CreateGistIndexOptionsSyntax(expression);
                case Algorithm.Spgist:
                    return new CreateSpgistIndexOptionsSyntax(expression);
                case Algorithm.Gin:
                    return new CreateGinIndexOptionsSyntax(expression);
                case Algorithm.Brin:
                    return new CreateBrinIndexOptionsSyntax(expression);
                default:
                    throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
            }
        }

        public static void Using(this ISupportAdditionalFeatures additionalFeatures, Algorithm algorithm)
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
