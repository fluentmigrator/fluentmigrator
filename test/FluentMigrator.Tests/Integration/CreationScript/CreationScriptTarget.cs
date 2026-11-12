#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;

using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Processors.DB2;
using FluentMigrator.Runner.Processors.Oracle;

using Microsoft.Data.SqlClient;

using MySql.Data.MySqlClient;

using Npgsql;

using NUnit.Framework;

using Oracle.ManagedDataAccess.Client;

namespace FluentMigrator.Tests.Integration.CreationScript
{
    /// <summary>
    /// A database in one of the integration-test containers: how FluentMigrator targets it, how to get a new empty
    /// database on that server, and how to run a script file on it the way the database's own tool would.
    /// </summary>
    public abstract class CreationScriptTarget
    {
        public static IReadOnlyList<CreationScriptTarget> All { get; } =
        [
            new SqlServerTarget(),
            new PostgresTarget(),
            new MySqlTarget(),
            new OracleTarget(),
            new FirebirdTarget(),
            new Db2Target(),
        ];

        /// <summary>
        /// The processor id, which is also what <c>IfDatabase</c> in the example matches.
        /// </summary>
        public abstract string Name { get; }

        public abstract IntegrationTestOptions.DatabaseServerOptions ServerOptions { get; }

        /// <summary>
        /// Whether the script separates statements with <c>GO</c>, as dotnet-fm's <c>--output</c> does for SQL Server.
        /// </summary>
        public virtual bool GoBetweenStatements => false;

        public abstract void AddProcessor(IMigrationRunnerBuilder builder);

        /// <summary>
        /// Creates a new, empty database (or schema or user, where that is the unit of isolation) on the configured
        /// server. Disposing the result drops it.
        /// </summary>
        public abstract EmptyDatabase CreateEmptyDatabase(string configuredConnectionString);

        /// <summary>
        /// Runs a script file's text, statement by statement where the database needs that, outside any transaction.
        /// </summary>
        public abstract void RunScript(string connectionString, string script);

        public override string ToString() => Name;

        protected static string NewDatabaseName() => "fmcs" + Guid.NewGuid().ToString("N").Substring(0, 12);

        protected static void Execute(DbConnection connection, string sql)
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 300;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new ScriptStatementException(sql, ex);
            }
        }

        protected static void Execute<TConnection>(string connectionString, params string[] statements)
            where TConnection : DbConnection, new()
        {
            using var connection = new TConnection { ConnectionString = connectionString };
            connection.Open();
            foreach (var statement in statements)
            {
                Execute(connection, statement);
            }
        }

        protected static void RunStatements<TConnection>(string connectionString, IEnumerable<string> statements)
            where TConnection : DbConnection, new()
        {
            Execute<TConnection>(connectionString, statements.Where(s => !IsOnlyComments(s)).ToArray());
        }

        /// <summary>
        /// The script interleaves the runner's messages, written as <c>/* ... */</c>, with the SQL.
        /// </summary>
        protected static bool IsOnlyComments(string sql)
        {
            var withoutComments = Regex.Replace(sql, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            withoutComments = Regex.Replace(withoutComments, @"--[^\n]*", string.Empty);
            return string.IsNullOrWhiteSpace(withoutComments);
        }

        /// <summary>
        /// Splits a script at each <c>;</c> outside string literals, quoted identifiers and comments, as psql, the
        /// mysql client and the Db2 command line processor do. Each statement then runs on its own, in autocommit.
        /// </summary>
        /// <remarks>
        /// The generators put several statements on one line for a multi-row insert or a multi-column drop, so
        /// splitting at line ends is not enough.
        /// </remarks>
        protected static IEnumerable<string> SplitStatements(string script)
        {
            var statement = new System.Text.StringBuilder();
            char? quote = null;

            for (var i = 0; i < script.Length; i++)
            {
                var c = script[i];
                var next = i + 1 < script.Length ? script[i + 1] : '\0';

                if (quote != null)
                {
                    statement.Append(c);
                    if (c == quote)
                    {
                        // A doubled quote is an escaped quote.
                        if (next == quote)
                        {
                            statement.Append(next);
                            i++;
                        }
                        else
                        {
                            quote = null;
                        }
                    }

                    continue;
                }

                if (c == '/' && next == '*')
                {
                    var end = script.IndexOf("*/", i + 2, StringComparison.Ordinal);
                    end = end < 0 ? script.Length : end + 2;
                    statement.Append(script, i, end - i);
                    i = end - 1;
                    continue;
                }

                if (c == '-' && next == '-')
                {
                    var end = script.IndexOf('\n', i);
                    end = end < 0 ? script.Length : end;
                    statement.Append(script, i, end - i);
                    i = end - 1;
                    continue;
                }

                if (c == ';')
                {
                    yield return statement.ToString();
                    statement.Clear();
                    continue;
                }

                if (c == '\'' || c == '"' || c == '`')
                {
                    quote = c;
                }

                statement.Append(c);
            }

            yield return statement.ToString();
        }

        private sealed class SqlServerTarget : CreationScriptTarget
        {
            public override string Name => "SqlServer2016";

            public override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.SqlServer2016;

            public override bool GoBetweenStatements => true;

            public override void AddProcessor(IMigrationRunnerBuilder builder) => builder.AddSqlServer2016();

            public override EmptyDatabase CreateEmptyDatabase(string configuredConnectionString)
            {
                var name = NewDatabaseName();
                Execute<SqlConnection>(configuredConnectionString, $"CREATE DATABASE [{name}]");

                return new EmptyDatabase(
                    new SqlConnectionStringBuilder(configuredConnectionString) { InitialCatalog = name }.ConnectionString,
                    () =>
                    {
                        SqlConnection.ClearAllPools();
                        Execute<SqlConnection>(
                            configuredConnectionString,
                            $"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE",
                            $"DROP DATABASE [{name}]");
                    });
            }

            // sqlcmd: each batch between GO lines is one command.
            public override void RunScript(string connectionString, string script)
            {
                var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                RunStatements<SqlConnection>(connectionString, batches);
            }
        }

        private sealed class PostgresTarget : CreationScriptTarget
        {
            public override string Name => "Postgres15_0";

            public override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.Postgres;

            public override void AddProcessor(IMigrationRunnerBuilder builder) => builder.AddPostgres15_0();

            public override EmptyDatabase CreateEmptyDatabase(string configuredConnectionString)
            {
                var name = NewDatabaseName();
                Execute<NpgsqlConnection>(configuredConnectionString, $"CREATE DATABASE \"{name}\"");

                return new EmptyDatabase(
                    new NpgsqlConnectionStringBuilder(configuredConnectionString) { Database = name }.ConnectionString,
                    () =>
                    {
                        NpgsqlConnection.ClearAllPools();
                        Execute<NpgsqlConnection>(configuredConnectionString, $"DROP DATABASE \"{name}\" WITH (FORCE)");
                    });
            }

            // psql sends each statement on its own, in autocommit. One multi-statement command would run them all in
            // an implicit transaction, where CREATE INDEX CONCURRENTLY fails.
            public override void RunScript(string connectionString, string script)
            {
                RunStatements<NpgsqlConnection>(connectionString, SplitStatements(script));
            }
        }

        private sealed class MySqlTarget : CreationScriptTarget
        {
            public override string Name => "MySql8";

            public override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.MySql;

            public override void AddProcessor(IMigrationRunnerBuilder builder) => builder.AddMySql8();

            // The container's application user may only use its own database. Testcontainers gives root the same
            // password.
            public override EmptyDatabase CreateEmptyDatabase(string configuredConnectionString)
            {
                var admin = new MySqlConnectionStringBuilder(configuredConnectionString) { UserID = "root" };
                var name = NewDatabaseName();
                Execute<MySqlConnection>(admin.ConnectionString, $"CREATE DATABASE `{name}`");

                return new EmptyDatabase(
                    new MySqlConnectionStringBuilder(admin.ConnectionString) { Database = name }.ConnectionString,
                    () =>
                    {
                        MySqlConnection.ClearAllPools();
                        Execute<MySqlConnection>(admin.ConnectionString, $"DROP DATABASE `{name}`");
                    });
            }

            // The mysql client sends each statement on its own.
            public override void RunScript(string connectionString, string script)
            {
                RunStatements<MySqlConnection>(connectionString, SplitStatements(script));
            }
        }

        private sealed class OracleTarget : CreationScriptTarget
        {
            public override string Name => "Oracle12CManaged";

            public override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.Oracle;

            public override void AddProcessor(IMigrationRunnerBuilder builder) => builder.AddOracle12CManaged();

            // In Oracle a schema is a user. Testcontainers gives SYSTEM the application user's password.
            public override EmptyDatabase CreateEmptyDatabase(string configuredConnectionString)
            {
                var configured = new OracleConnectionStringBuilder(configuredConnectionString);
                var admin = new OracleConnectionStringBuilder(configuredConnectionString) { UserID = "system" };
                var name = NewDatabaseName().ToUpperInvariant();

                Execute<OracleConnection>(
                    admin.ConnectionString,
                    $"CREATE USER {name} IDENTIFIED BY \"{configured.Password}\" DEFAULT TABLESPACE USERS QUOTA UNLIMITED ON USERS",
                    $"GRANT CREATE SESSION, CREATE TABLE, CREATE SEQUENCE, CREATE VIEW, CREATE TRIGGER, CREATE PROCEDURE, CREATE SYNONYM TO {name}");

                return new EmptyDatabase(
                    new OracleConnectionStringBuilder(configuredConnectionString) { UserID = name }.ConnectionString,
                    () =>
                    {
                        OracleConnection.ClearAllPools();
                        Execute<OracleConnection>(admin.ConnectionString, $"DROP USER {name} CASCADE");
                    });
            }

            // SQL*Plus runs one statement at a time, and Oracle rejects a trailing ';' on plain SQL. This is the
            // splitter FluentMigrator's Oracle processor uses for Execute.Script.
            public override void RunScript(string connectionString, string script)
            {
                RunStatements<OracleConnection>(connectionString, OracleSqlParser.SplitOracleSqlStatements(script));
            }
        }

        private sealed class FirebirdTarget : CreationScriptTarget
        {
            public override string Name => "Firebird";

            public override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.Firebird;

            public override void AddProcessor(IMigrationRunnerBuilder builder) => builder.AddFirebird();

            public override EmptyDatabase CreateEmptyDatabase(string configuredConnectionString)
            {
                var connectionString = new FbConnectionStringBuilder(configuredConnectionString)
                {
                    Database = NewDatabaseName() + ".fdb",
                }.ConnectionString;

                FbConnection.CreateDatabase(connectionString, pageSize: 16384, forcedWrites: true, overwrite: true);

                return new EmptyDatabase(
                    connectionString,
                    () =>
                    {
                        FbConnection.ClearAllPools();
                        FbConnection.DropDatabase(connectionString);
                    });
            }

            // isql's parser and batch runner, from the Firebird ADO.NET provider. Each statement commits, as with isql's
            // AUTODDL, because Firebird can't use a table in the transaction that created it.
            public override void RunScript(string connectionString, string script)
            {
                var parsed = new FbScript(script);

                // The runner's closing messages are comments after the last ';', which the parser can't classify.
                parsed.UnknownStatement += (_, e) =>
                {
                    if (IsOnlyComments(e.Statement.Text))
                    {
                        e.Ignore = true;
                    }
                };

                parsed.Parse();

                using var connection = new FbConnection(connectionString);
                connection.Open();

                var batch = new FbBatchExecution(connection);
                batch.AppendSqlStatements(parsed);

                string current = null;
                batch.CommandExecuting += (_, e) => current = e.SqlCommand?.CommandText;

                try
                {
                    batch.Execute(autoCommit: true);
                }
                catch (Exception ex)
                {
                    throw new ScriptStatementException(current, ex);
                }
            }
        }

        private sealed class Db2Target : CreationScriptTarget
        {
            public override string Name => "DB2";

            public override IntegrationTestOptions.DatabaseServerOptions ServerOptions => IntegrationTestOptions.Db2;

            public override void AddProcessor(IMigrationRunnerBuilder builder) => builder.AddDb2();

            // Creating a Db2 database takes minutes, so the unit of isolation is a schema: unqualified names go to the
            // connection's CurrentSchema.
            public override EmptyDatabase CreateEmptyDatabase(string configuredConnectionString)
            {
                var name = NewDatabaseName().ToUpperInvariant();
                using (var connection = CreateConnection(configuredConnectionString))
                {
                    connection.Open();
                    Execute(connection, $"CREATE SCHEMA {name}");
                }

                var builder = new DbConnectionStringBuilder { ConnectionString = configuredConnectionString };
                builder["CurrentSchema"] = name;

                return new EmptyDatabase(
                    builder.ConnectionString,
                    () =>
                    {
                        using var connection = CreateConnection(configuredConnectionString);
                        connection.Open();
                        Execute(connection, $"CALL SYSPROC.ADMIN_DROP_SCHEMA('{name}', NULL, 'ERRORSCHEMA', 'ERRORTABLE')");
                    });
            }

            // The Db2 command line processor runs one ';'-terminated statement at a time.
            public override void RunScript(string connectionString, string script)
            {
                using var connection = CreateConnection(connectionString);
                connection.Open();
                foreach (var statement in SplitStatements(script).Where(s => !IsOnlyComments(s)))
                {
                    Execute(connection, statement);
                }
            }

            // The Db2 provider is a different package on Windows and Linux; FluentMigrator finds it by reflection.
            private static DbConnection CreateConnection(string connectionString)
            {
                var connection = new Db2DbFactory(serviceProvider: null).Factory.CreateConnection();
                connection.ConnectionString = connectionString;
                return connection;
            }
        }
    }

    /// <summary>
    /// A database created for one test. Disposing it drops it.
    /// </summary>
    public sealed class EmptyDatabase : IDisposable
    {
        private readonly Action _drop;

        public EmptyDatabase(string connectionString, Action drop)
        {
            ConnectionString = connectionString;
            _drop = drop;
        }

        public string ConnectionString { get; }

        public void Dispose()
        {
            try
            {
                _drop();
            }
            catch (Exception ex)
            {
                // A database left behind doesn't make the result wrong; each run uses a new name.
                TestContext.Out.WriteLine($"Could not drop the test database: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// A statement of the script that the database rejected, with the statement in the message.
    /// </summary>
    public sealed class ScriptStatementException : Exception
    {
        public ScriptStatementException(string statement, Exception innerException)
            : base($"{innerException.Message}{Environment.NewLine}The statement was:{Environment.NewLine}{statement}", innerException)
        {
        }
    }
}
