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
    /// <summary>The database conventions for the table used to store the database version.</summary>
    public interface IVersionTableMetaData
    {
        /// <summary>The schema containing the table, or <see cref="string.Empty"/> for the default root schema.</summary>
        string SchemaName { get; }

        /// <summary>The name of the table.</summary>
        string TableName { get; }

        /// <summary>The name of the column containing the version number.</summary>
        string ColumnName { get; }

        /// <summary>The name of the unique index on the <see cref="ColumnName"/>.</summary>
        string UniqueIndexName { get; }
    }
}