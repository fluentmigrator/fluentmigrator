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

namespace FluentMigrator.VersionTableInfo
{
    /// <summary>The default database conventions for the table used to store the database version, which identify a `VersionInfo` table in the default root schema.</summary>
    public class DefaultVersionTableMetaData : IVersionTableMetaData
    {
        /// <summary>The schema containing the table (<see cref="string.Empty"/> for the default root schema).</summary>
        public virtual string SchemaName
        {
            get { return string.Empty; }
        }

        /// <summary>The name of the table.</summary>
        public virtual string TableName
        {
            get { return "VersionInfo"; }
        }

        /// <summary>The name of the column containing the version number.</summary>
        public virtual string ColumnName
        {
            get { return "Version"; }
        }

        /// <summary>The name of the unique index on the <see cref="IVersionTableMetaData.ColumnName"/>.</summary>
        public virtual string UniqueIndexName
        {
            get { return "UC_Version"; }
        }
    }
}
