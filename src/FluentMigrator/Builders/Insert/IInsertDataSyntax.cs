#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// Copyright (c) 2011, Grant Archibald
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

using System.Data;

namespace FluentMigrator.Builders.Insert
{
	public interface IInsertDataSyntax
	{
		IInsertDataSyntax Row(object dataAsAnonymousType);
      /// <summary>
      /// Insert data from a <see cref="DataTable"/> file
      /// </summary>
      /// <remarks>if the file ends in xml ist is assembled to be an xml serialized datatable</remarks>
      /// <param name="dataFile">The data file to read the <see cref="DataTable"/> from</param>
      /// <returns>Fluent interface instance to add further data</returns>
      IInsertDataSyntax DataTable(string dataFile);

      /// <summary>
      /// <para>If <c>True</c> then each insert statement should be individualy inserted as part of the migration.</para> 
      /// <para> If <c>False</c> then processors can combine mluiple inserts into a single insert</para> 
      /// </summary>
      /// <returns>Fluent interface instance to add further data</returns>
      IInsertDataSyntax InsertRowsSeparately();

      /// <summary>
      /// Insert the data with special handling for Identity (or Sequence) column
      /// </summary>
      /// <remarks>See the processor implementation on how this is implemeted</remarks>
      /// <returns>Fluent interface instance to add further data</returns>
	   IInsertDataSyntax WithIdentity();
	}
}