using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator.Builders.Delete.Constraint
{
    public interface IDeleteConstraintOnTableSyntax
    {
        void FromTable(string tableName);
    }
}
