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

namespace FluentMigrator
{
    /// <summary>
    /// The constraint type as supported by SQL Anywhere
    /// </summary>
    public enum SqlAnywhereConstraintType
    {
        /// <summary>
        /// The index/unique constraint should be clustered
        /// </summary>
        Clustered,

        /// <summary>
        /// The index/unique constraint should not be clustered
        /// </summary>
        NonClustered
    }
}
