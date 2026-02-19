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

namespace FluentMigrator.Postgres
{
    /// <summary>
    /// Provides extension methods for configuring PostgreSQL-specific index algorithms and additional features
    /// during the creation of database indexes.
    /// </summary>
    public static partial class PostgresExtensions
    {
        /// <summary>
        /// Configures the index to use the B-Tree algorithm for sorting and searching.
        /// </summary>
        /// <param name="expression">The index options syntax to configure.</param>
        /// <returns>An object that allows further configuration of the B-Tree index options.</returns>
        /// <exception cref="InvalidOperationException">Thrown if an index method has already been specified.</exception>
        /// <remarks>
        /// This method is specific to PostgreSQL and ensures that the index uses the B-Tree algorithm.
        /// </remarks>
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

        /// <summary>
        /// Configures the index to use the "Hash" algorithm in a PostgreSQL database.
        /// </summary>
        /// <param name="expression">
        /// The index options syntax to configure the index algorithm.
        /// </param>
        /// <returns>
        /// An object that allows further configuration of the hash index.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if an index method has already been specified.
        /// </exception>
        /// <remarks>
        /// This method is specific to PostgreSQL and is used to define a hash-based index.
        /// </remarks>
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

        /// <summary>
        /// Configures the index to use the GiST (Generalized Search Tree) algorithm in a PostgreSQL database.
        /// </summary>
        /// <param name="expression">The index options syntax to configure.</param>
        /// <returns>An object that allows further configuration of GiST-specific index options.</returns>
        /// <exception cref="InvalidOperationException">Thrown when an index method is already specified.</exception>
        /// <remarks>
        /// This method is specific to PostgreSQL and allows the creation of GiST indexes, which are suitable for complex data types
        /// and support various types of queries such as range, nearest neighbor, and full-text searches.
        /// </remarks>
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

        /// <summary>
        /// Configures the index to use the SP-GiST (Space-Partitioned Generalized Search Tree) algorithm in a PostgreSQL database.
        /// </summary>
        /// <param name="expression">The index options syntax to configure.</param>
        /// <returns>An object that allows further configuration of SP-GiST-specific index options.</returns>
        /// <exception cref="InvalidOperationException">Thrown if an index method has already been specified.</exception>
        /// <remarks>
        /// SP-GiST is a PostgreSQL indexing method optimized for certain types of data, such as hierarchical or spatial data.
        /// This method ensures that only one index method is applied to the index.
        /// </remarks>
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

        /// <summary>
        /// Configures the index to use the GIN (Generalized Inverted Index) algorithm in a PostgreSQL database.
        /// </summary>
        /// <param name="expression">The index options syntax to configure.</param>
        /// <returns>An object that allows further configuration of the GIN index options.</returns>
        /// <exception cref="InvalidOperationException">Thrown if an index method is already specified.</exception>
        /// <remarks>
        /// GIN indexes are particularly useful for indexing composite types, arrays, and full-text search data.
        /// </remarks>
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

        /// <summary>
        /// Configures the index to use the BRIN (Block Range INdex) algorithm in a PostgreSQL database.
        /// </summary>
        /// <param name="expression">The index options syntax to configure.</param>
        /// <returns>An object that allows further configuration of BRIN-specific index options.</returns>
        /// <remarks>
        /// BRIN indexes are designed to handle large tables efficiently by summarizing ranges of blocks.
        /// This method modifies the index creation process to utilize the BRIN algorithm.
        /// </remarks>
        /// <example>
        /// <code>
        /// builder.WithOptions().UsingBrin().PagesPerRange(90);
        /// </code>
        /// </example>
        public static ICreateBrinIndexOptionsSyntax UsingBrin(this ICreateIndexOptionsSyntax expression)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures;
            additionalFeatures.Using(Algorithm.Brin);
            return new CreateBrinIndexOptionsSyntax(expression);
        }

        /// <summary>
        /// Specifies the index algorithm to use when creating an index in a PostgreSQL database.
        /// </summary>
        /// <param name="expression">
        /// The index options syntax to which the algorithm will be applied.
        /// </param>
        /// <param name="algorithm">
        /// The algorithm to use for the index. Supported algorithms include:
        /// <see cref="Algorithm.BTree"/>, <see cref="Algorithm.Hash"/>, <see cref="Algorithm.Gist"/>, 
        /// <see cref="Algorithm.Spgist"/>, <see cref="Algorithm.Gin"/>, and <see cref="Algorithm.Brin"/>.
        /// </param>
        /// <returns>
        /// An object that provides additional options for configuring the index creation.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if an index method has already been specified.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the specified <paramref name="algorithm"/> is not recognized.
        /// </exception>
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

        /// <summary>
        /// Configures the specified <paramref name="additionalFeatures"/> to use the given <paramref name="algorithm"/> 
        /// for index creation in PostgreSQL.
        /// </summary>
        /// <param name="additionalFeatures">
        /// The object that supports additional features for index creation.
        /// </param>
        /// <param name="algorithm">
        /// The index algorithm to be used. Supported algorithms include <see cref="Algorithm.BTree"/>, 
        /// <see cref="Algorithm.Hash"/>, <see cref="Algorithm.Gist"/>, <see cref="Algorithm.Spgist"/>, 
        /// <see cref="Algorithm.Gin"/>, and <see cref="Algorithm.Brin"/>.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the <paramref name="additionalFeatures"/> parameter is <c>null</c>, or the method is not supported 
        /// for the provided <paramref name="additionalFeatures"/>.
        /// </exception>
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
