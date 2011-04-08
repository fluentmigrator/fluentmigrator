using System;
using System.Data;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
    internal class PostgresColumn : ColumnBase 
    {
        public PostgresColumn() :base(new PostgresTypeMap(), new PostgresFormatter()) { }

        protected override string FormatIdentity(ColumnDefinition column)
        {
            return string.Empty;
        }

        protected override string FormatPrimaryKey(ColumnDefinition column)
        {
            return column.IsPrimaryKey ? string.Format(", CONSTRAINT \"PK_{1}\" PRIMARY KEY (\"{0}\")", column.Name, column.TableName) : string.Empty;
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
                case SystemMethods.NewGuid:
                    //need to run the script share/contrib/uuid-ossp.sql to install the uuid_generate4 function
                    return "uuid_generate_v4()";
                case SystemMethods.CurrentDateTime:
                    return "now()";
            }

            throw new NotImplementedException();
        }

        protected override string FormatType(ColumnDefinition column)
        {
            if (column.IsIdentity)
            {
                if (column.Type == DbType.Int64)
                    return "bigserial";
                return "serial";
            }   

            return base.FormatType(column);
        }


        public string GetColumnType(ColumnDefinition column)
        {
            return FormatType(column);
        }

        protected override string FormatName(ColumnDefinition column)
        {
            return string.Format("\"{0}\"", column.Name);
        }

        protected override string GetPrimaryKeyConstraintName(System.Collections.Generic.IList<ColumnDefinition> primaryKeyColumns, string tableName)
        {
            var name = base.GetPrimaryKeyConstraintName(primaryKeyColumns, tableName);
            return "CONSTRAINT \"" + name.Substring("CONSTRAINT ".Length) + "\"";
        }
        
    }
}