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

namespace FluentMigrator.VersionTableInfo
{
    public class DefaultVersionTableMetaData : IVersionTableMetaData, IVersionTableMetaDataExtended
    {
        public virtual string SchemaName
        {
            get { return string.Empty; }
        }
  
        public virtual string TableName
        {
            get { return "VersionInfo"; }
        }
  
        public virtual string ColumnName
        {
            get { return "Version"; }
        }
  
        public virtual string UniqueIndexName
        {
            get { return "UC_Version"; }
        }
  
        public virtual string DescriptionColumnName
        {
            get { return "Description"; }
        }

        public virtual bool OwnsSchema
        {
            get { return true; }
        }
    }
}