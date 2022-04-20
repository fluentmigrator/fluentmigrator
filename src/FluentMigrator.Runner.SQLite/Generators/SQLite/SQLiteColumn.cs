using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;

namespace FluentMigrator.Runner.Generators.SQLite
{
    // ReSharper disable once InconsistentNaming
    internal class SQLiteColumn : ColumnBase
    {
        public SQLiteColumn()
            : base(new SQLiteTypeMap(), new SQLiteQuoter())
        {
            // Add UNIQUE before IDENTITY and after PRIMARY KEY
            ClauseOrder.Insert(ClauseOrder.Count - 2, FormatUniqueConstraint);
        }

        /// <inheritdoc />
        public override string Generate(IEnumerable<ColumnDefinition> columns, string tableName)
        {
            var colDefs = columns.ToList();
            var foreignKeyColumns = colDefs.Where(x => x.IsForeignKey && x.ForeignKey != null);
            var foreignKeyClauses = foreignKeyColumns
                .Select(x => ", " + FormatForeignKey(x.ForeignKey, GenerateForeignKeyName))
                .ToList();

            // As we generate FKs as part of the create table statement we need a way to prevent
            // these FK's from creating FK constraints (which SQLite doesn't support) so we prefix
            // the FK name and ignore anything in the "CreateConstraint" handler that has this name prefix
            foreach (var fk in foreignKeyColumns) {
                fk.ForeignKey.Name = "$$IGNORE$$_" + fk.ForeignKey.Name;
            }

            // Append foreign key definitions after all column definitions and the primary key definition
            return base.Generate(colDefs, tableName) + string.Concat(foreignKeyClauses);
        }

        /// <summary>
        /// Formats the unique SQL fragment
        /// </summary>
        /// <param name="column">The column definition</param>
        /// <returns>The formatted unique SQL fragment</returns>
        protected virtual string FormatUniqueConstraint(ColumnDefinition column)
        {
            // Define unique constraints on columns in addition to creating a unique index
            return column.IsUnique ? "UNIQUE" : string.Empty;
        }

        /// <inheritdoc />
        public override string FormatForeignKey(ForeignKeyDefinition foreignKey, Func<ForeignKeyDefinition, string> fkNameGeneration)
        {
            var fk2 = (ForeignKeyDefinition)foreignKey.Clone();

            // SQLite FK's must be within the same schema as the FK itself
            // so we'll remove the schema from the FK definition
            fk2.PrimaryTableSchema = string.Empty;

            return base.FormatForeignKey(fk2, fkNameGeneration);
        }

        /// <inheritdoc />
        protected override string FormatIdentity(ColumnDefinition column)
        {
            if (column.IsIdentity)
            {
                // SQLite only supports the concept of Identity in combination with a single integer primary key
                // see: http://www.sqlite.org/syntaxdiagrams.html#column-constraint syntax details
                if (!column.IsPrimaryKey && (!column.Type.HasValue || GetTypeMap(column.Type.Value, null, null) != "INTEGER"))
                {
                    throw new ArgumentException($"Cannot create identity constraint on column {column.Name}. SQLite only supports identity on a single integer, primary key column.");
                }

                return "AUTOINCREMENT";
            }

            return string.Empty;
        }

        /// <inheritdoc />
        protected override string FormatPrimaryKey(ColumnDefinition column)
        {
            return column.IsPrimaryKey ? "PRIMARY KEY" : string.Empty;
        }

        /// <inheritdoc />
        public override bool ShouldPrimaryKeysBeAddedSeparately(IEnumerable<ColumnDefinition> primaryKeyColumns)
        {
            // If there are no identity column then we can add as a separate constraint
            var pkColDefs = primaryKeyColumns.ToList();
            return !pkColDefs.Any(x => x.IsIdentity) && pkColDefs.Any(x => x.IsPrimaryKey);
        }
    }
}
