using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FluentMigrator.Tests
{
    public static class DataReaderExtensions
    {
        public static DataTable ToDataTable(this IDataReader dataReader) {
            var dataTable = new DataTable();
            dataTable.Load(dataReader);
            return dataTable;
        }
    }
}
