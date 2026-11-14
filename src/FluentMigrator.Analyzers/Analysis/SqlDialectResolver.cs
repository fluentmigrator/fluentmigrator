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

using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FluentMigrator.Analyzers.Analysis
{
    /// <summary>
    /// Determines which <see cref="SqlDialect"/> a SQL statement is written for.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two signals are available to a compile-time analyzer, in precedence order:
    /// </para>
    /// <list type="number">
    /// <item>
    /// <description>
    /// An <c>IfDatabase("...")</c> call in the receiver chain of the statement. This is the most
    /// specific signal available and scopes to exactly the statements it guards.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The <c>fluent_migrator_sql_dialect</c> <c>.editorconfig</c> key, which applies to a whole
    /// file, directory or project.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// Referenced runner packages are deliberately <em>not</em> used as a signal. Projects routinely
    /// reference several providers, so the inference would be wrong more often than useful.
    /// </para>
    /// </remarks>
    internal static class SqlDialectResolver
    {
        /// <summary>
        /// The <c>.editorconfig</c> key that pins the dialect for a file, directory or project.
        /// </summary>
        internal const string EditorConfigKey = "fluent_migrator_sql_dialect";

        private const string IfDatabaseMethodName = "IfDatabase";

        /// <summary>
        /// Resolves the dialect for a specific <c>Sql(...)</c> invocation.
        /// </summary>
        /// <param name="invocation">The invocation being analyzed</param>
        /// <param name="options">The analyzer options carrying <c>.editorconfig</c> values</param>
        /// <returns>The resolved dialect, or <see cref="SqlDialect.Unknown"/> when neither signal applies</returns>
        public static SqlDialect Resolve(InvocationExpressionSyntax invocation, AnalyzerOptions options)
        {
            var fromIfDatabase = FromIfDatabase(invocation);
            if (fromIfDatabase != SqlDialect.Unknown)
            {
                return fromIfDatabase;
            }

            return FromEditorConfig(invocation, options);
        }

        /// <summary>
        /// Walks the receiver chain looking for <c>IfDatabase("...")</c>.
        /// </summary>
        /// <remarks>
        /// Matches the common shape <c>IfDatabase("Postgres").Execute.Sql("...")</c>. When
        /// <c>IfDatabase</c> names more than one database, the dialect is only resolved if every
        /// name maps to the same dialect - <c>IfDatabase("SqlServer2012", "SqlServer2016")</c>
        /// resolves, <c>IfDatabase("SqlServer", "Postgres")</c> does not. Predicate-based overloads
        /// carry no usable name and are ignored.
        /// </remarks>
        private static SqlDialect FromIfDatabase(InvocationExpressionSyntax invocation)
        {
            ExpressionSyntax current = invocation;

            while (current != null)
            {
                switch (current)
                {
                    case InvocationExpressionSyntax candidate:
                        if (GetMethodName(candidate) == IfDatabaseMethodName)
                        {
                            return FromIfDatabaseArguments(candidate);
                        }

                        current = candidate.Expression;
                        break;

                    case MemberAccessExpressionSyntax memberAccess:
                        current = memberAccess.Expression;
                        break;

                    case ParenthesizedExpressionSyntax parenthesized:
                        current = parenthesized.Expression;
                        break;

                    default:
                        return SqlDialect.Unknown;
                }
            }

            return SqlDialect.Unknown;
        }

        private static SqlDialect FromIfDatabaseArguments(InvocationExpressionSyntax ifDatabase)
        {
            var arguments = ifDatabase.ArgumentList?.Arguments;
            if (arguments == null || arguments.Value.Count == 0)
            {
                return SqlDialect.Unknown;
            }

            var resolved = SqlDialect.Unknown;

            foreach (var argument in arguments.Value)
            {
                if (!(argument.Expression is LiteralExpressionSyntax literal) ||
                    !literal.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    // A predicate overload, a constant reference, or anything else we cannot read.
                    return SqlDialect.Unknown;
                }

                var dialect = FromProcessorId(literal.Token.ValueText);
                if (dialect == SqlDialect.Unknown)
                {
                    return SqlDialect.Unknown;
                }

                if (resolved == SqlDialect.Unknown)
                {
                    resolved = dialect;
                }
                else if (resolved != dialect)
                {
                    // Guarded for several dialects at once; nothing dialect-specific is safe.
                    return SqlDialect.Unknown;
                }
            }

            return resolved;
        }

        private static SqlDialect FromEditorConfig(SyntaxNode node, AnalyzerOptions options)
        {
            var tree = node?.SyntaxTree;
            if (tree == null || options == null)
            {
                return SqlDialect.Unknown;
            }

            var configOptions = options.AnalyzerConfigOptionsProvider.GetOptions(tree);
            if (!configOptions.TryGetValue(EditorConfigKey, out var value))
            {
                return SqlDialect.Unknown;
            }

            return FromProcessorId(value);
        }

        /// <summary>
        /// Maps a FluentMigrator processor id, or an <c>.editorconfig</c> value, onto a dialect.
        /// </summary>
        /// <remarks>
        /// Accepts the <c>ProcessorIdConstants</c> spellings that <c>IfDatabase</c> takes, including
        /// version-suffixed variants, since no supported version changes literal lexing.
        /// </remarks>
        internal static SqlDialect FromProcessorId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return SqlDialect.Unknown;
            }

            var normalized = value.Trim().Replace(" ", string.Empty).Replace("-", string.Empty).ToUpperInvariant();

            if (StartsWithAny(normalized, "SQLSERVER", "MSSQL", "TSQL"))
            {
                return SqlDialect.SqlServer;
            }

            // Checked before POSTGRES: Redshift is Postgres-derived but has no dollar quoting.
            if (StartsWithAny(normalized, "REDSHIFT"))
            {
                return SqlDialect.Redshift;
            }

            if (StartsWithAny(normalized, "POSTGRES", "PGSQL"))
            {
                return SqlDialect.Postgres;
            }

            if (StartsWithAny(normalized, "MYSQL", "MARIADB"))
            {
                return SqlDialect.MySql;
            }

            if (StartsWithAny(normalized, "ORACLE", "DOTCONNECTORACLE"))
            {
                return SqlDialect.Oracle;
            }

            if (StartsWithAny(normalized, "SQLITE"))
            {
                return SqlDialect.SQLite;
            }

            if (StartsWithAny(normalized, "FIREBIRD"))
            {
                return SqlDialect.Firebird;
            }

            if (StartsWithAny(normalized, "DB2", "IBMDB2"))
            {
                return SqlDialect.Db2;
            }

            if (StartsWithAny(normalized, "HANA"))
            {
                return SqlDialect.Hana;
            }

            if (StartsWithAny(normalized, "SNOWFLAKE"))
            {
                return SqlDialect.Snowflake;
            }

            return SqlDialect.Unknown;
        }

        private static bool StartsWithAny(string value, params string[] prefixes)
        {
            foreach (var prefix in prefixes)
            {
                if (value.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetMethodName(InvocationExpressionSyntax invocation)
        {
            switch (invocation.Expression)
            {
                case MemberAccessExpressionSyntax memberAccess:
                    return memberAccess.Name.Identifier.ValueText;
                case IdentifierNameSyntax identifier:
                    return identifier.Identifier.ValueText;
                case GenericNameSyntax generic:
                    return generic.Identifier.ValueText;
                default:
                    return null;
            }
        }
    }
}
