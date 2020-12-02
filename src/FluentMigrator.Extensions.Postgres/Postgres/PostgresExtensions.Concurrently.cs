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
        public static ICreateIndexOptionsSyntax AsConcurrently(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.AsConcurrently(true);
            return expression;
        }

        public static ICreateIndexOnColumnSyntax AsConcurrently(this ICreateIndexOnColumnSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.AsConcurrently(true);
            return expression;
        }

        public static ICreateIndexOptionsSyntax AsConcurrently(this ICreateIndexOptionsSyntax expression, bool isConcurrently)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.AsConcurrently(isConcurrently);
            return expression;
        }

        public static ICreateIndexOnColumnSyntax AsConcurrently(this ICreateIndexOnColumnSyntax expression, bool isConcurrently)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.AsConcurrently(isConcurrently);
            return expression;
        }

        internal static void AsConcurrently(this ISupportAdditionalFeatures additionalFeatures, bool isConcurrently)
        {
            if (additionalFeatures == null)
            {
                throw new InvalidOperationException(UnsupportedMethodMessage(nameof(Include), nameof(ISupportAdditionalFeatures)));
            }

            var asConcurrently = additionalFeatures.GetAdditionalFeature(Concurrently, () => new PostgresIndexConcurrentlyDefinition());
            asConcurrently.IsConcurrently = isConcurrently;
        }
    }
}
