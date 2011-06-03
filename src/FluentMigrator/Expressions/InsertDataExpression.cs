#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using FluentMigrator.Model;

namespace FluentMigrator.Expressions
{
   /// <summary>
   /// A expression repersentation that allows data to be inserted by a specified database processor
   /// </summary>
	public class InsertDataExpression : IMigrationExpression
	{
      ///<summary>
      /// Constructs a new instance of a <see cref="InsertDataExpression"/>
      ///</summary>
      public InsertDataExpression()
      {
         ReplacementValues = new Dictionary<object, object>();
      }

		private readonly List<InsertionDataDefinition> _rows = new List<InsertionDataDefinition>();
		public string SchemaName { get; set; }
		public string TableName { get; set; }

      /// <summary>
      /// The name of the data table to obtain data from
      /// </summary>
      public string DataTableFile { get; set; }

      /// <summary>
      /// <para>If <c>True</c> then each insert statement should be individualy inserted as part of the migration.</para> 
      /// <para> If <c>False</c> then processors can combine mluiple inserts into a single insert</para> 
      /// </summary>
      public bool InsertRowsSeparately { get; set; }

      /// <summary>
      /// Determines if handling indentity insert is required
      /// </summary>
      public bool WithIdentity { get; set; }

		public List<InsertionDataDefinition> Rows
		{
			get { return _rows; }
		}

      /// <summary>
      /// A dictionary of replacement values that should be applied when inserting the data <see cref="Rows"/> values
      /// </summary>
	   public Dictionary<object, object> ReplacementValues { get; private set; }

	   public void CollectValidationErrors(ICollection<string> errors)
		{
		}

		public void ExecuteWith(IMigrationProcessor processor)
		{
			processor.Process(this);
		}

		public IMigrationExpression Reverse()
		{
			var expression = new DeleteDataExpression
								{
									SchemaName = SchemaName,
									TableName = TableName
								};

			foreach (var row in Rows)
			{
				var dataDefinition = new DeletionDataDefinition();
				dataDefinition.AddRange(row);

				expression.Rows.Add(dataDefinition);
			}

			return expression;
		}

		public void ApplyConventions(IMigrationConventions conventions)
		{
         if ( ! string.IsNullOrEmpty(DataTableFile))
         {
            Rows.AddRange(GetData(Path.Combine(conventions.GetWorkingDirectory(), DataTableFile)));
         }
		}

       private static IEnumerable<InsertionDataDefinition> GetData(string pathToDataTable)
      {
         var data = new List<InsertionDataDefinition>();

         if (!File.Exists(pathToDataTable))
            return data;

          var xsd = pathToDataTable.Replace(".xml", ".xsd");

          if (!File.Exists(xsd))
             return data;

         var dataSet = new DataSet();
          dataSet.ReadXmlSchema(xsd);
          dataSet.ReadXml(pathToDataTable);

          var dataAsDataTable = dataSet.Tables[0];

         foreach (DataRow dr in dataAsDataTable.Rows)
         {
            var dataRow = new InsertionDataDefinition();
            dataRow.AddRange(from DataColumn column in dataAsDataTable.Columns
                             select new KeyValuePair<string, object>(column.ColumnName, dr[column.ColumnName]));
            data.Add(dataRow);
         }
            
         return data;
      }
	}
}