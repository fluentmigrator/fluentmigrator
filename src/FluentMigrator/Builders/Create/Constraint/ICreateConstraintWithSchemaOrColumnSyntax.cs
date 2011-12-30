using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator.Builders.Create.Constraint
{
    public interface ICreateConstraintWithSchemaOrColumnSyntax : ICreateConstraintColumnsSyntax,ICreateConstraintWithSchemaSyntax
    {
    }
}
