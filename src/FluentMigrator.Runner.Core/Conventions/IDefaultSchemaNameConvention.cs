#region License
// Copyright (c) 2007-2018, FluentMigrator Project
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

namespace FluentMigrator.Runner.Conventions
{
    /// <summary>
    /// A convention that returns the default schema name depending on the original schema name
    /// </summary>
    public interface IDefaultSchemaNameConvention
    {
        /// <summary>
        /// Returns the default schema name depending on the original schema name
        /// </summary>
        /// <param name="originalSchemaName">The original schema name</param>
        /// <returns>Returns the <paramref name="originalSchemaName"/> when the
        /// default schema name is null or empty and returns the new default
        /// schema name when the <paramref name="originalSchemaName"/> is null
        /// or empty</returns>
        string GetSchemaName(string originalSchemaName);
    }
}
