namespace FluentMigrator.Runner.Generators.DB2
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    using FluentMigrator.Model;
    using FluentMigrator.Runner.Generators.Base;

    internal class Db2Column : ColumnBase
    {
        #region Constructors

        public Db2Column()
            : base(new Db2TypeMap(), new Db2Quoter())
        {
            this.ClauseOrder = new List<Func<ColumnDefinition, string>> { FormatString, FormatType, this.FormatCCSID, this.FormatNullable, this.FormatDefaultValue, this.FormatIdentity };
            this.AlterClauseOrder = new List<Func<ColumnDefinition, string>> { FormatType, this.FormatCCSID, this.FormatNullable, this.FormatDefaultValue, this.FormatIdentity };
        }

        #endregion Constructors

        #region Properties

        public List<Func<ColumnDefinition, string>> AlterClauseOrder
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public string FormatAlterDefaultValue(string column, object defaultValue)
        {
            return defaultValue is SystemMethods
                ? this.FormatSystemMethods((SystemMethods)defaultValue)
                : Quoter.QuoteValue(defaultValue);
        }

        public string GenerateAlterClause(ColumnDefinition column)
        {
            if (column.IsIdentity)
            {
                throw new NotSupportedException("Altering an identity column is not supported.");
            }

            var alterClauses = AlterClauseOrder.Aggregate(new StringBuilder(), (acc, newRow) =>
            {
                var clause = newRow(column);
                if (acc.Length == 0)
                {
                    acc.Append(newRow(column));
                }
                else if (!string.IsNullOrEmpty(clause))
                {
                    acc.Append(clause.PadLeft(clause.Length + 1));
                }

                return acc;
            });

            return string.Format(
                "ALTER COLUMN {0} SET DATA TYPE {1}",
                Quoter.QuoteColumnName(column.Name),
                alterClauses);
        }

        protected virtual string FormatCCSID(ColumnDefinition column)
        {
            if (column.Type == null)
            {
                return string.Empty;
            }

            var dbType = (DbType)column.Type;

            if (DbType.String.Equals(dbType) || DbType.StringFixedLength.Equals(dbType))
            {
                // Force UTF-16 on double-byte character types.
                return "CCSID 1200";
            }

            return string.Empty;
        }

        protected override string FormatDefaultValue(ColumnDefinition column)
        {
            var isCreate = column.GetAdditionalFeature<bool>("IsCreateColumn", false);

            if (isCreate && (column.DefaultValue is ColumnDefinition.UndefinedDefaultValue))
            {
                return "DEFAULT";
            }

            if (column.DefaultValue is ColumnDefinition.UndefinedDefaultValue)
            {
                return string.Empty;
            }

            // see if this is for a system method
            if (!(column.DefaultValue is SystemMethods))
            {
                return "DEFAULT " + this.Quoter.QuoteValue(column.DefaultValue);
            }

            var method = this.FormatSystemMethods((SystemMethods)column.DefaultValue);
            if (string.IsNullOrEmpty(method))
            {
                return string.Empty;
            }

            return "DEFAULT " + method;
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return column.IsIdentity ? "AS IDENTITY" : string.Empty;
        }

        protected override string FormatNullable(ColumnDefinition column)
        {
            if (column.IsNullable.HasValue && column.IsNullable.Value)
            {
                return string.Empty;
            }

            return "NOT NULL";
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.CurrentUTCDateTime:
                    return "(CURRENT_TIMESTAMP - CURRENT_TIMEZONE)";
                case SystemMethods.CurrentDateTime:
                    return "CURRENT_TIMESTAMP";
                case SystemMethods.CurrentUser:
                    return "USER";
            }

            throw new NotImplementedException();
        }

        #endregion Methods
    }
}