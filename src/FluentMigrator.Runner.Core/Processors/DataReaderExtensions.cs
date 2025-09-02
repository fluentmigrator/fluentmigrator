#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using System;
using System.Data;

namespace FluentMigrator.Runner.Processors
{
    /// <summary>
    /// Provides extension methods for <see cref="System.Data.IDataReader"/> to facilitate reading data into <see cref="System.Data.DataSet"/> and <see cref="System.Data.DataTable"/>.
    /// </summary>
    public static class DataReaderExtensions
    {
        /// <summary>
        /// Reads all result sets from the specified <see cref="IDataReader"/> and populates them into a <see cref="DataSet"/>.
        /// </summary>
        /// <param name="reader">The <see cref="IDataReader"/> instance to read data from.</param>
        /// <returns>A <see cref="DataSet"/> containing all the result sets read from the <paramref name="reader"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is <c>null</c>.</exception>
        /// <remarks>
        /// This method iterates through all result sets available in the <paramref name="reader"/> and adds each result set as a <see cref="DataTable"/> to the returned <see cref="DataSet"/>.
        /// </remarks>
        public static DataSet ReadDataSet(this IDataReader reader)
        {
            var result = new DataSet();
            do
            {
                result.Tables.Add(reader.ReadTable());
            } while (reader.NextResult());

            return result;
        }

        /// <summary>
        /// Reads the current result set from the specified <see cref="IDataReader"/> and populates it into a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="reader">The <see cref="IDataReader"/> instance to read data from.</param>
        /// <returns>A <see cref="DataTable"/> containing the data from the current result set of the <paramref name="reader"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is <c>null</c>.</exception>
        /// <remarks>
        /// This method reads the current result set from the <paramref name="reader"/> and creates a <see cref="DataTable"/> 
        /// with columns based on the schema of the result set. It then populates the table with all rows from the result set.
        /// </remarks>
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

        /// <summary>
        /// Creates and adds columns to the specified <see cref="DataTable"/> based on the schema of the current result set
        /// in the provided <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="IDataReader"/> instance to retrieve column schema information from.</param>
        /// <param name="table">The <see cref="DataTable"/> to which the columns will be added.</param>
        /// <exception cref="ArgumentNullException">Thrown if either <paramref name="reader"/> or <paramref name="table"/> is <c>null</c>.</exception>
        /// <remarks>
        /// This method iterates through all fields in the current result set of the <paramref name="reader"/> and creates
        /// corresponding <see cref="DataColumn"/> objects, which are then added to the specified <paramref name="table"/>.
        /// </remarks>
        private static void CreateColumns(this IDataReader reader, DataTable table)
        {
            for (var i = 0; i != reader.FieldCount; ++i)
            {
                table.Columns.Add(reader.CreateColumn(i));
            }
        }

        /// <summary>
        /// Creates a <see cref="DataColumn"/> for the specified field index in the <see cref="IDataReader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="IDataReader"/> instance to retrieve the column information from.</param>
        /// <param name="fieldIndex">The zero-based index of the field for which to create the <see cref="DataColumn"/>.</param>
        /// <returns>A <see cref="DataColumn"/> representing the field at the specified index in the <paramref name="reader"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="reader"/> is <c>null</c>.</exception>
        /// <exception cref="IndexOutOfRangeException">Thrown if the <paramref name="fieldIndex"/> is out of range.</exception>
        /// <remarks>
        /// This method retrieves the field name and type from the <paramref name="reader"/> at the specified <paramref name="fieldIndex"/> 
        /// and uses them to create a new <see cref="DataColumn"/>.
        /// </remarks>
        private static DataColumn CreateColumn(this IDataReader reader, int fieldIndex)
        {
            var fieldName = reader.GetName(fieldIndex);
            var fieldType = reader.GetFieldType(fieldIndex);
            return new DataColumn(fieldName, fieldType);
        }
    }
}
