using System;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;
using FluentMigrator.Runner.Generators.PostgresBase;
using System.Data;

namespace FluentMigrator.Runner.Generators.Postgres 
{
    internal class PostgresColumn : PostgresBaseColumn 
    {
        public PostgresColumn() : base(new PostgresTypeMap(), new PostgresQuoter()) 
        {
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
    }
}

