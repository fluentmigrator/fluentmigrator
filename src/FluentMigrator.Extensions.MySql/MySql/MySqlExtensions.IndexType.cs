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

using FluentMigrator.Builder.Create.Index;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;

namespace FluentMigrator.MySql
{
    public static partial class MySqlExtensions
    {
        public static ICreateBTreeIndexOptionsSyntax UsingBTree(this ICreateIndexOptionsSyntax expression)
        {
            if (expression is ICreateIndexMethodOptionsSyntax)
            {
                throw new InvalidOperationException("Only can have one index method.");
            }

            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Model.IndexType.BTree);
            return new CreateBTreeIndexOptionsSyntax(expression);
        }

        public static ICreateHashIndexOptionSyntax UsingHash(this ICreateIndexOptionsSyntax expression)
        {
            if (expression is ICreateIndexMethodOptionsSyntax)
            {
                throw new InvalidOperationException("Only can have one index method.");
            }

            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Model.IndexType.Hash);
            return new CreateHashIndexOptionSyntax(expression);
        }

        public static ICreateIndexMethodOptionsSyntax Using(this ICreateIndexOptionsSyntax expression, IndexType indexType)
        {
            if (expression is ICreateIndexMethodOptionsSyntax)
            {
                throw new InvalidOperationException("Only can have one index method.");
            }

            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(indexType);
            switch (indexType)
            {
                case Model.IndexType.BTree:
                    return new CreateBTreeIndexOptionsSyntax(expression);
                case Model.IndexType.Hash:
                    return new CreateHashIndexOptionSyntax(expression);
                default:
                    throw new ArgumentOutOfRangeException(nameof(indexType), indexType, null);
            }
        }

        public static void Using(this ISupportAdditionalFeatures additionalFeatures, IndexType indexType)
        {
            if (additionalFeatures == null)
            {
                throw new InvalidOperationException(UnsupportedMethodMessage($"Using{indexType}Algorithm", nameof(ISupportAdditionalFeatures)));
            }

            var algorithmDefinition = additionalFeatures.GetAdditionalFeature(IndexType,  new MySqlIndexTypeDefinition());
            algorithmDefinition.IndexType = indexType;
        }
    }
}
