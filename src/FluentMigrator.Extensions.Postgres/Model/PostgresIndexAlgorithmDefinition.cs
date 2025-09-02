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

namespace FluentMigrator.Model
{
    /// <summary>
    /// Represents the definition of an index algorithm specific to PostgreSQL.
    /// </summary>
    /// <remarks>
    /// This class is used to specify the algorithm type for PostgreSQL indexes, 
    /// such as BTree, Hash, Gist, Gin, Brin, or Spgist. It implements the <see cref="ICloneable"/> 
    /// interface to allow creating a shallow copy of the instance.
    /// </remarks>
    public class PostgresIndexAlgorithmDefinition
        : ICloneable
    {
        /// <summary>
        /// Gets or sets the algorithm used for creating a PostgreSQL index.
        /// </summary>
        /// <remarks>
        /// The algorithm specifies the type of index to be created, such as BTree, Hash, Gist, Spgist, Gin, or Brin.
        /// This property is utilized to define the index method in PostgreSQL migrations.
        /// </remarks>
        public virtual Algorithm Algorithm { get; set; }

        /// <summary>
        /// Creates a shallow copy of the current <see cref="PostgresIndexAlgorithmDefinition"/> instance.
        /// </summary>
        /// <returns>A shallow copy of the current instance.</returns>
        /// <remarks>
        /// This method uses <see cref="object.MemberwiseClone"/> to create a copy of the instance.
        /// </remarks>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    /// <summary>
    /// Represents the algorithm used for creating an index in a PostgreSQL database.
    /// </summary>
    /// <remarks>
    /// The algorithm determines the type of index structure to be used, which can affect
    /// performance and storage characteristics depending on the use case.
    /// </remarks>
    /// <summary>
    /// A balanced tree index, which is the default index type in PostgreSQL.
    /// </summary>
    /// <summary>
    /// A hash index, optimized for equality comparisons.
    /// </summary>
    /// <summary>
    /// A GiST (Generalized Search Tree) index, suitable for complex data types and range queries.
    /// </summary>
    /// <summary>
    /// A SP-GiST (Space-Partitioned GiST) index, designed for partitioned data structures.
    /// </summary>
    /// <summary>
    /// A GIN (Generalized Inverted Index), ideal for indexing composite values or full-text search.
    /// </summary>
    /// <summary>
    /// A BRIN (Block Range INdex), efficient for large tables with sequentially ordered data.
    /// </summary>
    public enum Algorithm
    {
        /// <summary>
        /// Represents the B-Tree indexing algorithm, which is the default index type in PostgreSQL.
        /// </summary>
        /// <remarks>
        /// B-Tree indexes are balanced tree structures that are optimized for a wide range of queries, 
        /// including equality and range queries. This algorithm is commonly used for most indexing needs 
        /// due to its versatility and performance.
        /// </remarks>
        BTree,
        /// <summary>
        /// Represents the Hash index algorithm used in PostgreSQL.
        /// </summary>
        /// <remarks>
        /// The Hash index algorithm is optimized for equality comparisons and is not suitable for range queries.
        /// It is typically used for indexing columns where equality checks are predominant.
        /// </remarks>
        Hash,
        /// <summary>
        /// Represents the GiST (Generalized Search Tree) index algorithm, 
        /// which is used in PostgreSQL for creating flexible and extensible indexing structures.
        /// </summary>
        /// <remarks>
        /// GiST indexes are particularly useful for indexing complex data types such as geometrical data,
        /// full-text search, and other non-standard data types.
        /// </remarks>
        Gist,
        /// <summary>
        /// Represents the SP-GiST (Space-Partitioned Generalized Search Tree) index algorithm in PostgreSQL.
        /// </summary>
        /// <remarks>
        /// SP-GiST is a PostgreSQL index type designed for data that can be partitioned in a space, such as geometric data.
        /// It supports fast lookups and is particularly useful for non-balanced data distributions.
        /// </remarks>
        Spgist,
        /// <summary>
        /// Represents the GIN (Generalized Inverted Index) algorithm for PostgreSQL indexes.
        /// </summary>
        /// <remarks>
        /// GIN is used for indexing composite types, arrays, and full-text search data.
        /// It is particularly efficient for queries involving containment operators.
        /// </remarks>
        Gin,
        /// <summary>
        /// Represents the BRIN (Block Range INdex) algorithm for PostgreSQL indexes.
        /// </summary>
        /// <remarks>
        /// BRIN indexes are designed to handle very large tables efficiently by summarizing data in blocks.
        /// They are particularly useful for columns with natural ordering, such as timestamps or sequential IDs.
        /// </remarks>
        Brin
    }
}
