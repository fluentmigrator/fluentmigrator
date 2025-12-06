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
    /// <summary>
    /// Extension methods for MySQL syntax.
    /// </summary>
    public static partial class MySqlExtensions
    {
        /// <summary>
        /// Configures the CREATE INDEX expression to use a MySQL B-Tree index type.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Only one index method is allowed per CREATE INDEX expression.</exception>
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

        /// <summary>
        /// Configures the CREATE INDEX expression to use a MySQL Hash index type.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Only one index method is allowed per CREATE INDEX expression.</exception>
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

        /// <summary>
        /// Configures the CREATE INDEX expression to use a MySQL index type.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="indexType">The Index Type to use.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Only one index method is allowed per CREATE INDEX expression.</exception>
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

        /// <summary>
        /// MySQL-specific helper method for configuring the CREATE INDEX expression to use a MySQL B-Tree index type.
        /// </summary>
        /// <param name="additionalFeatures"></param>
        /// <param name="indexType">The Index Type to use.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Only one index method is allowed per CREATE INDEX expression.</exception>
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
