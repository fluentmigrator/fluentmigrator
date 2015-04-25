using System;
using System.Collections.Generic;
using System.Data;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Base;
using FluentMigrator.Runner.Generators.PostgresBase;

namespace FluentMigrator.Runner.Generators.Redshift
{
    internal class RedshiftColumn : PostgresBaseColumn
    {
        public RedshiftColumn() : base(new RedshiftTypeMap(), new RedshiftQuoter()) 
        {
        }

        protected override string FormatSystemMethods(SystemMethods systemMethod)
        {
            switch (systemMethod)
            {
            case SystemMethods.CurrentDateTime:
                return "now()";
            case SystemMethods.CurrentUTCDateTime:
                return "(now() at time zone 'UTC')";
            case SystemMethods.CurrentUser:
                return "current_user";
            }

            throw new NotImplementedException(string.Format("System method {0} is not implemented.", systemMethod));
        }

    }
}