#region License
// Copyright (c) 2025, Fluent Migrator Project
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

using System;

namespace FluentMigrator
{
    public static class GeneratorIdConstants
    {
        public const string DB2 = nameof(DB2);
        public const string Db2ISeries = "DB2 iSeries";
        public const string Firebird = nameof(Firebird);
        [Obsolete("Hana support will go away unless someone in the community steps up to provide support.")]
        public const string Hana = nameof(Hana);
        public const string Jet = nameof(Jet);
        public const string MariaDB = nameof(MariaDB);
        public const string MySql = nameof(MySql);
        public const string MySql4 = nameof(MySql4);
        public const string MySql5 = nameof(MySql5);
        public const string MySql8 = nameof(MySql8);
        public const string Oracle = nameof(Oracle);
        public const string OracleManaged = nameof(OracleManaged);
        public const string Oracle12c = nameof(Oracle12c);
        public const string Postgres = nameof(Postgres);
        public const string Postgres92 = nameof(Postgres92);
        public const string PostgreSQL = nameof(PostgreSQL);
        public const string PostgreSQL9_2 = nameof(PostgreSQL9_2);
        public const string PostgreSQL10_0 = nameof(PostgreSQL10_0);
        public const string PostgreSQL11_0 = nameof(PostgreSQL11_0);
        public const string PostgreSQL15_0 = nameof(PostgreSQL15_0);
        public const string Redshift = nameof(Redshift);
        public const string Snowflake = nameof(Snowflake);
        public const string SQLite = nameof(SQLite);
        public const string SqlServer = nameof(SqlServer);
        public const string SqlServer2000 = nameof(SqlServer2000);
        public const string SqlServer2005 = nameof(SqlServer2005);
        public const string SqlServer2008 = nameof(SqlServer2008);
        public const string SqlServer2012 = nameof(SqlServer2012);
        public const string SqlServer2014 = nameof(SqlServer2014);
        public const string SqlServer2016 = nameof(SqlServer2016);
    }
}
