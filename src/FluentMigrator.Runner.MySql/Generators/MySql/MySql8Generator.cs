#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System.Collections.Generic;
using System.Text;

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.MySql;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.MySql
{
    public class MySql8Generator : MySql5Generator
    {
        public MySql8Generator()
            : this(new MySqlQuoter())
        {
        }

        public MySql8Generator(
            [NotNull] MySqlQuoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        public MySql8Generator(
            [NotNull] MySqlQuoter quoter,
            [NotNull] IMySqlTypeMap typeMap)
            : this(quoter, typeMap, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        public MySql8Generator(
            [NotNull] MySqlQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : this(
                new MySqlColumn(new MySql8TypeMap(), quoter),
                quoter,
                new EmptyDescriptionGenerator(),
                generatorOptions)
        {
        }

        public MySql8Generator(
            [NotNull] MySqlQuoter quoter,
            [NotNull] IMySqlTypeMap typeMap,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new MySqlColumn(typeMap, quoter), quoter, new EmptyDescriptionGenerator(), generatorOptions)
        {
        }

        protected MySql8Generator(
            [NotNull] IColumn column,
            [NotNull] IQuoter quoter,
            [NotNull] IDescriptionGenerator descriptionGenerator,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(column, quoter, descriptionGenerator, generatorOptions)
        {
        }

        public override string Generate(CreateIndexExpression expression)
        {
            var query = new StringBuilder("CREATE");

            if (expression.Index.IsUnique)
            {
                query.Append(" UNIQUE");
            }

            var indexMethod = GetIndexType(expression);

            query.AppendFormat(
                " INDEX {0}{1} ON {2}",
                Quoter.QuoteIndexName(expression.Index.Name),
                indexMethod == IndexType.BTree ? string.Empty : $" USING {indexMethod.ToString().ToUpperInvariant()}",
                Quoter.QuoteTableName(expression.Index.TableName, expression.Index.SchemaName)
            );


            query.Append(" (");
            var first = true;
            foreach (var column in expression.Index.Columns)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    query.Append(", ");
                }

                query.Append(Quoter.QuoteColumnName(column.Name));

                switch (indexMethod)
                {
                    // Doesn't support ASC/DESC neither nulls sorts
                    case IndexType.Hash:
                        continue;
                }

                query.Append(column.Direction == Direction.Ascending ? " ASC" : " DESC");
            }

            return query.Append(");")
                .ToString();
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.MySql8;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases =>
        [
            GeneratorIdConstants.MySql8, GeneratorIdConstants.MySql, GeneratorIdConstants.MariaDB
        ];


        protected virtual IndexType GetIndexType(CreateIndexExpression expression)
        {
            var algorithm =
                expression.GetAdditionalFeature<MySqlIndexTypeDefinition>(MySqlExtensions.IndexType);
            if (algorithm == null)
            {
                return IndexType.BTree;
            }

            return algorithm.IndexType;
        }
        public override string Generate(RenameColumnExpression expression)
        {
            return string.Format("ALTER TABLE {0} CHANGE {1} {2} ", Quoter.QuoteTableName(expression.TableName), Quoter.QuoteColumnName(expression.OldName), Quoter.QuoteColumnName(expression.NewName));
        }
    }
}
