using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator.Builders.Create.Constraint
{
    public interface ICreateConstraintColumnsSyntax
    {
        void Column(string columnName);
        void Columns(string[] columnNames);
    }
}
