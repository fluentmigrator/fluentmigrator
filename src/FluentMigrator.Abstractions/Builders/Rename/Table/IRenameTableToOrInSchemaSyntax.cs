#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

namespace FluentMigrator.Builders.Rename.Table
{
    /// <summary>
    /// Specify the new table name or schema of the table to rename
    /// </summary>
    public interface IRenameTableToOrInSchemaSyntax : IRenameTableToSyntax
    {
        /// <summary>
        /// Specify the tables schema name
        /// </summary>
        /// <param name="schemaName">The schema name</param>
        /// <returns>The next step</returns>
        IRenameTableToSyntax InSchema(string schemaName);
    }
}
