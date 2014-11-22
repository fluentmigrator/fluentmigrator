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
using FluentMigrator.VersionTableInfo;

namespace FluentMigrator.Tests.Unit
{
	[VersionTableMetaData]
	public class TestVersionTableMetaData : IVersionTableMetaData, IVersionTableMetaDataExtended
	{
		public const string TABLENAME = "testVersionTableName";
		public const string COLUMNNAME = "testColumnName";
		public const string UNIQUEINDEXNAME = "testUniqueIndexName";
        public const string DESCRIPTIONCOLUMNNAME = "testDescriptionColumnName";
	    public const string APPLIEDONCOLUMNNAME = "testAppliedOnColumnName";

		public TestVersionTableMetaData()
		{
			SchemaName = "testSchemaName";
            OwnsSchema = true;
		}

		public string SchemaName { get; set; }

        public string TableName
        {
			get { return TABLENAME; }
		}

		public string ColumnName
		{
			get { return COLUMNNAME; }
		}

		public string UniqueIndexName
		{
			get { return UNIQUEINDEXNAME; }
		}

	    public string AppliedOnColumnName
	    {
	        get { return APPLIEDONCOLUMNNAME; }
	    }

	    public string DescriptionColumnName
        {
            get { return DESCRIPTIONCOLUMNNAME; }
        }

	    public bool OwnsSchema { get; set; }
	}
}

