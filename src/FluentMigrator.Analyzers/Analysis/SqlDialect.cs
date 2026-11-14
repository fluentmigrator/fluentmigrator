#region License
// Copyright (c) 2024, Fluent Migrator Project
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

namespace FluentMigrator.Analyzers.Analysis
{
    /// <summary>
    /// The SQL dialect a statement is written for, insofar as an analyzer can determine it.
    /// </summary>
    /// <remarks>
    /// Only the distinctions that change <em>string literal lexing</em> are modelled. Version
    /// variants collapse into their family, because no supported version of any family changes how a
    /// literal is delimited.
    /// </remarks>
    public enum SqlDialect
    {
        /// <summary>
        /// The dialect could not be determined. Only dialect-independent diagnostics are reported.
        /// </summary>
        Unknown = 0,

        /// <summary>Microsoft SQL Server (T-SQL).</summary>
        SqlServer,

        /// <summary>PostgreSQL.</summary>
        Postgres,

        /// <summary>Amazon Redshift. Postgres-derived, but without dollar quoting.</summary>
        Redshift,

        /// <summary>MySQL or MariaDB.</summary>
        MySql,

        /// <summary>Oracle Database.</summary>
        Oracle,

        /// <summary>SQLite.</summary>
        SQLite,

        /// <summary>Firebird.</summary>
        Firebird,

        /// <summary>IBM Db2, including iSeries.</summary>
        Db2,

        /// <summary>SAP HANA.</summary>
        Hana,

        /// <summary>Snowflake.</summary>
        Snowflake,
    }
}
