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


namespace FluentMigrator.Builders.IfDatabase
{
    /// <summary>
    /// Provides a null implmentation of a procesor that does not do any work
    /// </summary>
    public class NullIfDatabaseProcessor : IQuerySchema
    {
        public bool SequenceExists(string schemaName, string sequenceName)
        {
            return false;
        }

        public bool DefaultValueExists(string schemaName, string tableName, string columnName, object defaultValue)
        {
            return false;
        }

        public string DatabaseType { get { return "Unknown"; } }

        public bool SchemaExists(string schemaName)
        {
            return false;
        }

        public bool TableExists(string schemaName, string tableName)
        {
            return false;
        }

        public bool ColumnExists(string schemaName, string tableName, string columnName)
        {
            return false;
        }

        public bool ConstraintExists(string schemaName, string tableName, string constraintName)
        {
            return false;
        }

        public bool IndexExists(string schemaName, string tableName, string indexName)
        {
            return false;
        }
    }
}