using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;
using FluentMigrator.Runner.Processors.Firebird;

namespace FluentMigrator.Runner.Generators.Firebird
{
    internal class FirebirdColumn : ColumnBase
    {
        protected FirebirdOptions FBOptions { get; private set; }

        public FirebirdColumn(FirebirdOptions fbOptions) : base(new FirebirdTypeMap(), new FirebirdQuoter()) 
        {
            if (fbOptions == null)
                throw new ArgumentNullException("fbOptions");
            FBOptions = fbOptions;

            //In firebird DEFAULT clause precedes NULLABLE clause
            ClauseOrder = new List<Func<ColumnDefinition, string>> { FormatString, FormatType, FormatDefaultValue, FormatNullable, FormatPrimaryKey, FormatIdentity };
        }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            //Identity not supported
           return string.Empty;
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.NewGuid:
                    return "gen_uuid()";
                case SystemMethods.CurrentDateTime:
                    return "CURRENT_TIMESTAMP";
            }

            throw new NotImplementedException();
        }

        protected override string GetPrimaryKeyConstraintName(IEnumerable<ColumnDefinition> primaryKeyColumns, string tableName)
        {
            string primaryKeyName = primaryKeyColumns.Select(x => x.PrimaryKeyName).FirstOrDefault();

            if (string.IsNullOrEmpty(primaryKeyName))
            {
                return string.Empty;
            }
            else if (primaryKeyName.Length > FirebirdOptions.MaxNameLength)
            {
                if (!FBOptions.TruncateLongNames)
                    throw new ArgumentException(String.Format("Name too long: {0}", primaryKeyName));
                primaryKeyName = primaryKeyName.Substring(0, Math.Min(FirebirdOptions.MaxNameLength, primaryKeyName.Length));
            }

            return string.Format("CONSTRAINT {0} ", Quoter.QuoteIndexName(primaryKeyName));
        }

        public virtual string GenerateForTypeAlter(ColumnDefinition column)
        {
            return FormatType(column);
        }

        public virtual string GenerateForDefaultAlter(ColumnDefinition column)
        {
            return FormatDefaultValue(column);
        }
    }
}