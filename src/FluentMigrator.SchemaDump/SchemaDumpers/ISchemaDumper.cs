using System.Collections.Generic;
using FluentMigrator.Model;

namespace FluentMigrator.SchemaDump.SchemaDumpers
{
    interface ISchemaDumper
    {
        IList<TableDefinition> ReadDbSchema();
    }
}
