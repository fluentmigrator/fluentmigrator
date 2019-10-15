#region License
//
// Copyright (c) 2019, Fluent Migrator Project
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

using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;
using FluentMigrator.Postgres;
using FluentMigrator.Runner.Generators.Postgres;

using JetBrains.Annotations;

using System;

namespace FluentMigrator.Runner.Generators.Postgres10
{
    internal class Postgres10Column : PostgresColumn
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Postgres10Column"/> class.
        /// </summary>
        /// <param name="quoter">The Postgres quoter.</param>
        /// <param name="typeMap">The Postgres type map.</param>
        public Postgres10Column([NotNull] PostgresQuoter quoter, ITypeMap typeMap)
            : base(quoter, typeMap)
        {
        }

        private static string GetIdentityString(ColumnDefinition column)
        {
            string generationType;

            switch (column.GetAdditionalFeature(PostgresExtensions.IdentityGeneration, PostgresGenerationType.ByDefault))
            {
                case PostgresGenerationType.Always:
                    generationType = "ALWAYS";
                    break;
                case PostgresGenerationType.ByDefault:
                    generationType = "BY DEFAULT";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return $"GENERATED {generationType} AS IDENTITY";
        }

        /// <inheritdoc />
        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? GetIdentityString(column) : string.Empty;
        }

        public override string GenerateAlterClauses(ColumnDefinition column)
        {
            var clauses = new System.Collections.Generic.List<string>();
            foreach (var action in AlterClauseOrder)
            {
                string columnClause = action(column);
                if (!string.IsNullOrEmpty(columnClause))
                    clauses.Add(string.Format("ALTER COLUMN {0} {1}", Quoter.QuoteColumnName(column.Name), columnClause));
            }

            return string.Join(", ", clauses.ToArray());
        }

        /// <inheritdoc />
        protected override string FormatType(ColumnDefinition column)
        {
            if (!column.Type.HasValue)
            {
                return column.CustomType;
            }

            return GetTypeMap(column.Type.Value, column.Size, column.Precision);
        }
    }
}
