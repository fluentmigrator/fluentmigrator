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

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using FluentMigrator.Generation;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators.Base
{
    /// <summary>
    /// The base class for column definitions
    /// </summary>
    public abstract class ColumnBase<TTypeMap> : IColumn
        where TTypeMap : ITypeMap
    {
        private readonly TTypeMap _typeMap;

        /// <summary>
        /// Gets or sets the clause order
        /// </summary>
        protected IList<Func<ColumnDefinition, string>> ClauseOrder { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnBase{TTypeMap}"/> class.
        /// </summary>
        /// <param name="typeMap">The type map</param>
        /// <param name="quoter">The quoter</param>
        protected ColumnBase(TTypeMap typeMap, IQuoter quoter)
        {
            _typeMap = typeMap;
            Quoter = quoter;
            ClauseOrder = new List<Func<ColumnDefinition, string>>
            {
                FormatString,
                FormatType,
                FormatCollation,
                FormatExpression,
                FormatNullable,
                FormatDefaultValue,
                FormatPrimaryKey,
                FormatIdentity
            };
        }

        /// <summary>
        /// The default foreign key constraint format
        /// </summary>
        public virtual string ForeignKeyConstraint => "{0}FOREIGN KEY ({1}) REFERENCES {2} ({3}){4}{5}";

        /// <summary>
        /// Gets the quoter
        /// </summary>
        protected IQuoter Quoter { get; }

        /// <summary>
        /// Gets the formatted type from the type map
        /// </summary>
        /// <param name="value">The database type</param>
        /// <param name="size">The size (or precision)</param>
        /// <param name="precision">The precision (or scale)</param>
        /// <returns>The formatted column type</returns>
        protected string GetTypeMap(DbType value, int? size, int? precision)
        {
            return _typeMap.GetTypeMap(value, size, precision);
        }

        /// <summary>
        /// Formats the column name
        /// </summary>
        /// <param name="column">The column definition</param>
        /// <returns>The (probably) quoted column name</returns>
        public virtual string FormatString(ColumnDefinition column)
        {
            return Quoter.QuoteColumnName(column.Name);
        }

        /// <summary>
        /// Formats the column type
        /// </summary>
        /// <param name="column">The column definition</param>
        /// <returns>The formatted column type</returns>
        protected virtual string FormatType(ColumnDefinition column)
        {
            if (!column.Type.HasValue)
            {
                return column.CustomType;
            }

            return GetTypeMap(column.Type.Value, column.Size, column.Precision);
        }

        /// <summary>
        /// Formats a computed column type definition
        /// </summary>
        protected virtual string FormatExpression(ColumnDefinition column)
        {
            return column.Expression == null ? null : $"GENERATED ALWAYS AS ({column.Expression}){(column.ExpressionStored ? " STORED" : "")}";
        }

        /// <summary>
        /// Formats the (not) null constraint
        /// </summary>
        /// <param name="column">The column definition</param>
        /// <returns>The formatted (not) null constraint</returns>
        protected virtual string FormatNullable(ColumnDefinition column)
        {
            if (column.IsNullable == true)
            {
                return string.Empty;
            }

            return "NOT NULL";
        }

        /// <summary>
        /// Formats the column default value
        /// </summary>
        /// <param name="column">The column definition</param>
        /// <returns>The formatted column default value</returns>
        protected virtual string FormatDefaultValue(ColumnDefinition column)
        {
            if (column.DefaultValue is ColumnDefinition.UndefinedDefaultValue)
            {
                return string.Empty;
            }

            // see if this is for a system method
            if (column.DefaultValue is SystemMethods methods)
            {
                var method = Quoter.QuoteValue(methods);
                if (string.IsNullOrEmpty(method))
                {
                    return string.Empty;
                }

                return "DEFAULT " + method;
            }

            return "DEFAULT " + Quoter.QuoteValue(column.DefaultValue);
        }

        /// <summary>
        /// Formats the identity SQL fragment
        /// </summary>
        /// <param name="column">The column definition</param>
        /// <returns>The formatted identity SQL fragment</returns>
        protected abstract string FormatIdentity(ColumnDefinition column);

        /// <summary>
        /// Formats the primary key constraint directly attached to the column
        /// </summary>
        /// <param name="column">The column definition</param>
        /// <returns>The primary key constraint SQL fragment</returns>
        protected virtual string FormatPrimaryKey(ColumnDefinition column)
        {
            //Most Generators allow for adding primary keys as a constraint
            return string.Empty;
        }

        /// <summary>
        /// Formats the collation of a text column
        /// </summary>
        /// <param name="column">The column definition</param>
        /// <returns>The SQL fragment</returns>
        protected virtual string FormatCollation(ColumnDefinition column)
        {
            if (!string.IsNullOrEmpty(column.CollationName))
            {
                return "COLLATE " + column.CollationName;
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public virtual string FormatCascade(string onWhat, Rule rule)
        {
            var action = "NO ACTION";
            switch (rule)
            {
                case Rule.None:
                    return "";
                case Rule.Cascade:
                    action = "CASCADE";
                    break;
                case Rule.SetNull:
                    action = "SET NULL";
                    break;
                case Rule.SetDefault:
                    action = "SET DEFAULT";
                    break;
            }

            return string.Format(" ON {0} {1}", onWhat, action);
        }

        /// <inheritdoc />
        public virtual string GenerateForeignKeyName(ForeignKeyDefinition foreignKey)
        {
            return string.Format("FK_{0}_{1}", foreignKey.PrimaryTable.Substring(0, 5), foreignKey.ForeignTable.Substring(0, 5));
        }

        /// <inheritdoc />
        public virtual string FormatForeignKey(ForeignKeyDefinition foreignKey, Func<ForeignKeyDefinition, string> fkNameGeneration)
        {
            if (foreignKey.PrimaryColumns.Count != foreignKey.ForeignColumns.Count)
            {
                throw new ArgumentException("Number of primary columns and secondary columns must be equal");
            }

            var constraintName = string.IsNullOrEmpty(foreignKey.Name)
                ? fkNameGeneration(foreignKey)
                : foreignKey.Name;

            var primaryColumns = new List<string>();
            var foreignColumns = new List<string>();
            foreach (var column in foreignKey.PrimaryColumns)
            {
                primaryColumns.Add(Quoter.QuoteColumnName(column));
            }

            foreach (var column in foreignKey.ForeignColumns)
            {
                foreignColumns.Add(Quoter.QuoteColumnName(column));
            }

            var constraintClause = string.IsNullOrEmpty(constraintName)
                ? string.Empty
                : $"CONSTRAINT {Quoter.QuoteConstraintName(constraintName)} ";

            return string.Format(
                ForeignKeyConstraint,
                constraintClause,
                string.Join(", ", foreignColumns.ToArray()),
                Quoter.QuoteTableName(foreignKey.PrimaryTable, foreignKey.PrimaryTableSchema),
                string.Join(", ", primaryColumns.ToArray()),
                FormatCascade("DELETE", foreignKey.OnDelete),
                FormatCascade("UPDATE", foreignKey.OnUpdate)
            );
        }

        /// <inheritdoc />
        public virtual string Generate(ColumnDefinition column)
        {
            var clauses = new List<string>();

            foreach (var action in ClauseOrder)
            {
                var clause = action(column);
                if (!string.IsNullOrEmpty(clause))
                {
                    clauses.Add(clause);
                }
            }

            return string.Join(" ", clauses.ToArray());
        }

        /// <inheritdoc />
        public virtual string Generate(IEnumerable<ColumnDefinition> columns, string tableName)
        {
            var primaryKeyString = string.Empty;

            //if more than one column is a primary key or the primary key is given a name, then it needs to be added separately

            //CAUTION: this must execute before we set the values of primarykey to false; Beware of yield return
            var colDefs = columns.ToList();
            var primaryKeyColumns = colDefs.Where(x => x.IsPrimaryKey);

            var pkColDefs = primaryKeyColumns.ToList();
            if (ShouldPrimaryKeysBeAddedSeparately(pkColDefs))
            {
                primaryKeyString = AddPrimaryKeyConstraint(tableName, pkColDefs);
                foreach (var column in colDefs) { column.IsPrimaryKey = false; }
            }

            return string.Join(", ", colDefs.Select(x => Generate(x)).ToArray()) + primaryKeyString;
        }

        /// <summary>
        /// Returns a value indicating whether the primary key constraint should be added separately
        /// </summary>
        /// <param name="primaryKeyColumns">The primary key column definitions</param>
        /// <returns><c>true</c> when the primary key constraint should be added separately</returns>
        public virtual bool ShouldPrimaryKeysBeAddedSeparately(IEnumerable<ColumnDefinition> primaryKeyColumns)
        {
            //By default always try to add primary keys as a separate constraint if any exist
            return primaryKeyColumns.Any(x => x.IsPrimaryKey);
        }

        /// <summary>
        /// Creates the primary key constraint SQL fragment
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="primaryKeyColumns">The primary key column definitions</param>
        /// <returns>The SQL fragment</returns>
        public virtual string AddPrimaryKeyConstraint(string tableName, IEnumerable<ColumnDefinition> primaryKeyColumns)
        {
            var pkColDefs = primaryKeyColumns.ToList();
            var keyColumns = string.Join(", ", pkColDefs.Select(x => Quoter.QuoteColumnName(x.Name)).ToArray());

            return string.Format(", {0}PRIMARY KEY ({1})", GetPrimaryKeyConstraintName(pkColDefs, tableName), keyColumns);
        }

        /// <summary>
        /// Gets the name of the primary key constraint. Some Generators may need to override if the constraint name is limited
        /// </summary>
        /// <param name="primaryKeyColumns">The primary key columns</param>
        /// <param name="tableName">The table name</param>
        /// <returns>The constraint clause</returns>
        protected virtual string GetPrimaryKeyConstraintName(IEnumerable<ColumnDefinition> primaryKeyColumns, string tableName)
        {
            var primaryKeyName = primaryKeyColumns.Select(x => x.PrimaryKeyName).FirstOrDefault();

            if (string.IsNullOrEmpty(primaryKeyName))
            {
                return string.Empty;
            }

            return string.Format("CONSTRAINT {0} ", Quoter.QuoteIndexName(primaryKeyName));
        }
    }
}
