#region License
// Copyright (c) 2018, FluentMigrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Data;

namespace FluentMigrator.Runner.Processors
{
    public static class DataReaderExtensions
    {
        public static DataSet ReadDataSet(this IDataReader reader)
        {
            var result = new DataSet();
            do
            {
                result.Tables.Add(reader.ReadTable());
            } while (reader.NextResult());

            return result;
        }

        public static DataTable ReadTable(this IDataReader reader)
        {
            var table = new DataTable();

            if (reader.Read())
            {
                reader.CreateColumns(table);

                var values = new object[reader.FieldCount];

                do
                {
                    reader.GetValues(values);
                    table.Rows.Add(values);
                } while (reader.Read());
            }

            return table;
        }

        private static void CreateColumns(this IDataReader reader, DataTable table)
        {
            for (var i = 0; i != reader.FieldCount; ++i)
            {
                table.Columns.Add(reader.CreateColumn(i));
            }
        }

        private static DataColumn CreateColumn(this IDataReader reader, int fieldIndex)
        {
            var fieldName = reader.GetName(fieldIndex);
            var fieldType = reader.GetFieldType(fieldIndex);
            return new DataColumn(fieldName, fieldType);
        }
    }
}
