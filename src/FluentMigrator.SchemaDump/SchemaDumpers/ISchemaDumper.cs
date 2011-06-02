using System.Collections.Generic;
using System.Data;
using FluentMigrator.Model;

namespace FluentMigrator.SchemaDump.SchemaDumpers {
   public interface ISchemaDumper {
      IList<TableDefinition> ReadDbSchema();
      IList<ViewDefinition> ReadViews();
      DataSet ReadTableData(string schemaName, string tableName);
   }
}
