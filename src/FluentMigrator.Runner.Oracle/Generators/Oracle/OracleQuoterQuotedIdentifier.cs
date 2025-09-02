#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

namespace FluentMigrator.Runner.Generators.Oracle
{
    /// <summary>
    /// Provides an implementation of <see cref="OracleQuoterBase"/> that enforces quoted identifiers
    /// for Oracle-specific SQL expressions.
    /// </summary>
    /// <remarks>
    /// This class is used to ensure that all identifiers, such as table and column names, are quoted
    /// in Oracle SQL statements, which is particularly useful when dealing with reserved keywords or
    /// special characters in identifiers.
    /// </remarks>
    public class OracleQuoterQuotedIdentifier : OracleQuoterBase
    {
    }
}
