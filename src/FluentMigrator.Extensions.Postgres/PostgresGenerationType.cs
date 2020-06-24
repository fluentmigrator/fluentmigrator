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
    /// Default value generation strategy (Identity) types. The clauses ALWAYS and BY DEFAULT determine how the sequence value is given precedence over a user-specified value in an INSERT statement.
    /// </summary>
    public enum PostgresGenerationType
    {
        /// <summary>
        /// A user-specified value is only accepted if the INSERT statement specifies OVERRIDING SYSTEM VALUE.
        /// </summary>
        Always,
        /// <summary>
        /// The user-specified value takes precedence.
        /// </summary>
        ByDefault,
    }
}
