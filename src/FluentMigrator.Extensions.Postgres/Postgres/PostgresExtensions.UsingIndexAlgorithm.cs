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
        public static ICreateIndexOptionsSyntax UsingBTree(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Algorithm.BTree);
            return expression;
        }

        public static ICreateIndexOptionsSyntax UsingHash(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Algorithm.Hash);
            return expression;
        }

        public static ICreateIndexOptionsSyntax UsingGist(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Algorithm.Gist);
            return expression;
        }

        public static ICreateIndexOptionsSyntax UsingSpgist(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Algorithm.Spgist);
            return expression;
        }

        public static ICreateIndexOptionsSyntax UsingGin(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Algorithm.Gin);
            return expression;
        }

        public static ICreateIndexOptionsSyntax UsingBrin(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Algorithm.Brin);
            return expression;
        }

        public static ICreateIndexOptionsSyntax Using(this ICreateIndexOptionsSyntax expression, Algorithm algorithm)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(algorithm);
            return expression;
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
