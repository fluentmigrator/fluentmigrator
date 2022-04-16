using System;
using System.Collections.Generic;
using System.Data;
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
                .Select(x => ", " + FormatForeignKey(x.ForeignKey, GenerateForeignKeyName));

            // Append foreign key definitions after all column definitions and the primary key definition
            return base.Generate(colDefs, tableName) + string.Concat(foreignKeyClauses);
        }

        protected virtual string FormatUniqueConstraint(ColumnDefinition column)
        {
            // Define unique constraints on columns in addition to creating a unique index
            return column.IsUnique ? "UNIQUE" : string.Empty;
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
                    throw new ArgumentException("SQLite only supports identity on a single integer, primary key coulmns");
                }

                return "AUTOINCREMENT";
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public override bool ShouldPrimaryKeysBeAddedSeparately(IEnumerable<ColumnDefinition> primaryKeyColumns)
        {
            //If there are no identity column then we can add as a separate constrint
            var pkColDefs = primaryKeyColumns.ToList();
            return !pkColDefs.Any(x => x.IsIdentity) && pkColDefs.Any(x => x.IsPrimaryKey);
        }

        /// <inheritdoc />
        protected override string FormatPrimaryKey(ColumnDefinition column)
        {
            return column.IsPrimaryKey ? "PRIMARY KEY" : string.Empty;
        }
    }
}
