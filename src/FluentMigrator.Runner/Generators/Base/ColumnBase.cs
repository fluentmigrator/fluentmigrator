using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators.Base
{
    internal abstract class ColumnBase : IColumn
    {
        private readonly ITypeMap _typeMap;
        private readonly IQuoter _quoter;
        protected IList<Func<ColumnDefinition, string>> ClauseOrder { get; set; }

        public ColumnBase(ITypeMap typeMap, IQuoter quoter)
        {
            _typeMap = typeMap;
            _quoter = quoter;
            ClauseOrder = new List<Func<ColumnDefinition, string>> { FormatString, FormatType, FormatCollation, FormatNullable, FormatDefaultValue, FormatPrimaryKey, FormatIdentity };
        }

        protected string GetTypeMap(DbType value, int size, int precision)
        {
            return _typeMap.GetTypeMap(value, size, precision);
        }

        protected IQuoter Quoter
        {
            get { return _quoter; }
        }

        public virtual string FormatString(ColumnDefinition column)
        {
            return _quoter.QuoteColumnName(column.Name);
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

        public string Generate(IEnumerable<ColumnDefinition> columns, string tableName)
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

            return String.Join(", ", columns.Select(x => Generate(x)).ToArray()) + primaryKeyString;
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
