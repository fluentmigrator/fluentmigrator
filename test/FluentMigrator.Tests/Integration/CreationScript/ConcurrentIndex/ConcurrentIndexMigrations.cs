#region License
//
// Copyright (c) 2026, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Postgres;
using FluentMigrator.Runner.Conventions;

namespace FluentMigrator.Tests.Integration.CreationScript.ConcurrentIndex
{
    /// <summary>
    /// The migration <see cref="CreateTableIfNotExistsConcurrentIndexTests"/> runs on PostgreSQL: a table created with
    /// <c>IfNotExists()</c> and an index declared on a column, which PostgreSQL creates in a <c>DO</c> block, and an index
    /// on the same table built with <c>CREATE INDEX CONCURRENTLY</c>, which can't run in a <c>DO</c> block.
    /// </summary>
    public static class ConcurrentIndexMigrations
    {
        /// <summary>
        /// Keeps this migration out of runs that scan the whole test assembly.
        /// </summary>
        public const string Tag = "ConcurrentIndex";

        public const string Table = "FmGadget";

        /// <summary>
        /// Declared on a column with <c>.Indexed(...)</c>, so it belongs to the table and is in the <c>DO</c> block.
        /// </summary>
        public const string TableIndex = "IX_FmGadget_Name";

        /// <summary>
        /// Declared with <c>Create.Index(...)</c> and <c>AsConcurrently()</c>, a separate expression.
        /// </summary>
        public const string ConcurrentIndex = "IX_FmGadget_Code";

        public static long[] Versions { get; } = [1];
    }

    /// <summary>
    /// <c>CREATE INDEX CONCURRENTLY</c> can't run in a transaction, so the migration has none.
    /// </summary>
    [Tags(ConcurrentIndexMigrations.Tag)]
    [Migration(1, TransactionBehavior.None, "Gadgets, unless the table is already there, and an index built concurrently")]
    public class M0001_CreateGadgetIfNotExistsWithAConcurrentIndex : ForwardOnlyMigration
    {
        public override void Up()
        {
            Create.Table(ConcurrentIndexMigrations.Table).IfNotExists()
                .WithColumn("Id").AsInt32().NotNullable().PrimaryKey("PK_FmGadget")
                .WithColumn("Name").AsString(100).NotNullable().Indexed(ConcurrentIndexMigrations.TableIndex)
                .WithColumn("Code").AsString(20).Nullable();

            Create.Index(ConcurrentIndexMigrations.ConcurrentIndex).OnTable(ConcurrentIndexMigrations.Table)
                .OnColumn("Code").Ascending()
                .WithOptions().AsConcurrently();
        }
    }

    /// <summary>
    /// A convention that builds a table's indexes concurrently, as a team that deploys without downtime might. It
    /// reaches the indexes the table declares with <c>.Indexed()</c> and <c>.Unique()</c> too, which the fluent syntax
    /// can't make concurrent.
    /// </summary>
    /// <remarks>
    /// It is limited to one table: the runner creates its own version table's index in a transaction, where
    /// <c>CREATE INDEX CONCURRENTLY</c> fails.
    /// </remarks>
    public sealed class ConcurrentIndexesConvention : IIndexConvention
    {
        private readonly string _tableName;

        public ConcurrentIndexesConvention(string tableName)
        {
            _tableName = tableName;
        }

        public IIndexExpression Apply(IIndexExpression expression)
        {
            if (expression.Index.TableName == _tableName)
            {
                expression.Index.AdditionalFeatures[PostgresExtensions.Concurrently] = new PostgresIndexConcurrentlyDefinition { IsConcurrently = true };
            }

            return expression;
        }
    }
}
