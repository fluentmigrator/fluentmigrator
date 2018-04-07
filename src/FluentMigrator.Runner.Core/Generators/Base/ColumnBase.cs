#region License
// Copyright (c) 2007-2018, Sean Chambers and the FluentMigrator Project
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

using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators.Base
{
    public abstract class ColumnBase : IColumn
    {
        private readonly ITypeMap _typeMap;
        protected IList<Func<ColumnDefinition, string>> ClauseOrder { get; set; }

        public ColumnBase(ITypeMap typeMap, IQuoter quoter)
        {
            _typeMap = typeMap;
            Quoter = quoter;
            ClauseOrder = new List<Func<ColumnDefinition, string>> { FormatString, FormatType, FormatCollation, FormatNullable, FormatDefaultValue, FormatPrimaryKey, FormatIdentity };
        }

        public virtual string ForeignKeyConstraint => "{0}FOREIGN KEY ({1}) REFERENCES {2} ({3}){4}{5}";

        protected IQuoter Quoter { get; }

        protected string GetTypeMap(DbType value, int size, int precision)
        {
            return _typeMap.GetTypeMap(value, size, precision);
        }

        public virtual string FormatString(ColumnDefinition column)
        {
            return Quoter.QuoteColumnName(column.Name);
        }

        protected virtual string FormatType(ColumnDefinition column)
        {
            if (!column.Type.HasValue)
                return column.CustomType;

            return GetTypeMap(column.Type.Value, column.Size, column.Precision);
        }

        protected virtual string FormatNullable(ColumnDefinition column)
        {
            if (column.IsNullable.HasValue && column.IsNullable.Value) {
                return string.Empty;
            }
            else {
                return "NOT NULL";
            }
        }

        protected virtual string FormatDefaultValue(ColumnDefinition column)
        {
            if (column.DefaultValue is ColumnDefinition.UndefinedDefaultValue)
                return string.Empty;

            // see if this is for a system method
            if (column.DefaultValue is SystemMethods)
            {
                string method = FormatSystemMethods((SystemMethods)column.DefaultValue);
                if (string.IsNullOrEmpty(method))
                    return string.Empty;

                return "DEFAULT " + method;
            }

            return "DEFAULT " + Quoter.QuoteValue(column.DefaultValue);
        }

        protected abstract string FormatIdentity(ColumnDefinition column);

        protected abstract string FormatSystemMethods(SystemMethods systemMethod);

        protected virtual string FormatPrimaryKey(ColumnDefinition column)
        {
            //Most Generators allow for adding primary keys as a constrint
            return string.Empty;
        }

        protected virtual string FormatCollation(ColumnDefinition column)
        {
            if (!string.IsNullOrEmpty(column.CollationName))
            {
                return "COLLATE " + column.CollationName;
            }
            else
            {
                return string.Empty;
            }
        }

        public virtual string FormatCascade(string onWhat, Rule rule)
        {
            string action = "NO ACTION";
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

        public virtual string GenerateForeignKeyName(ForeignKeyDefinition foreignKey)
        {
            return string.Format("FK_{0}_{1}", foreignKey.PrimaryTable.Substring(0, 5), foreignKey.ForeignTable.Substring(0, 5));
        }

        public virtual string FormatForeignKey(ForeignKeyDefinition foreignKey, Func<ForeignKeyDefinition, string> fkNameGeneration)
        {
            if (foreignKey.PrimaryColumns.Count != foreignKey.ForeignColumns.Count)
            {
                throw new ArgumentException("Number of primary columns and secondary columns must be equal");
            }

            string constraintName = string.IsNullOrEmpty(foreignKey.Name)
                ? fkNameGeneration(foreignKey)
                : foreignKey.Name;

            List<string> primaryColumns = new List<string>();
            List<string> foreignColumns = new List<string>();
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
                String.Join(", ", foreignColumns.ToArray()),
                Quoter.QuoteTableName(foreignKey.PrimaryTable),
                String.Join(", ", primaryColumns.ToArray()),
                FormatCascade("DELETE", foreignKey.OnDelete),
                FormatCascade("UPDATE", foreignKey.OnUpdate)
            );
        }

        public virtual string Generate(ColumnDefinition column)
        {
            var clauses = new List<string>();

            foreach (var action in ClauseOrder)
            {
                string clause = action(column);
                if (!string.IsNullOrEmpty(clause))
                    clauses.Add(clause);
            }

            return string.Join(" ", clauses.ToArray());
        }

        public virtual string Generate(IEnumerable<ColumnDefinition> columns, string tableName)
        {
            string primaryKeyString = string.Empty;

            //if more than one column is a primary key or the primary key is given a name, then it needs to be added separately

            //CAUTION: this must execute before we set the values of primarykey to false; Beware of yield return
            IEnumerable<ColumnDefinition> primaryKeyColumns = columns.Where(x => x.IsPrimaryKey);

            if (ShouldPrimaryKeysBeAddedSeparately(primaryKeyColumns))
            {
                primaryKeyString = AddPrimaryKeyConstraint(tableName, primaryKeyColumns);
                foreach (ColumnDefinition column in columns) { column.IsPrimaryKey = false; }
            }

            return string.Join(", ", columns.Select(x => Generate(x)).ToArray()) + primaryKeyString;
        }

        public virtual bool ShouldPrimaryKeysBeAddedSeparately(IEnumerable<ColumnDefinition> primaryKeyColumns)
        {
            //By default always try to add primary keys as a separate constraint if any exist
            return primaryKeyColumns.Any(x => x.IsPrimaryKey);
        }

        public virtual string AddPrimaryKeyConstraint(string tableName, IEnumerable<ColumnDefinition> primaryKeyColumns)
        {
            string keyColumns = String.Join(", ", primaryKeyColumns.Select(x => Quoter.QuoteColumnName(x.Name)).ToArray());

            return String.Format(", {0}PRIMARY KEY ({1})", GetPrimaryKeyConstraintName(primaryKeyColumns, tableName), keyColumns);
        }

        /// <summary>
        /// Gets the name of the primary key constraint. Some Generators may need to override if the constraint name is limited
        /// </summary>
        /// <returns></returns>
        protected virtual string GetPrimaryKeyConstraintName(IEnumerable<ColumnDefinition> primaryKeyColumns, string tableName)
        {
            string primaryKeyName = primaryKeyColumns.Select(x => x.PrimaryKeyName).FirstOrDefault();

            if (string.IsNullOrEmpty(primaryKeyName))
            {
                return string.Empty;
            }

            return string.Format("CONSTRAINT {0} ", Quoter.QuoteIndexName(primaryKeyName));
        }
    }
}
