#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using FluentMigrator.Expressions;
using FluentMigrator.Infrastructure;
using FluentMigrator.Postgres.Builder.SecurityLabel;

namespace FluentMigrator.Postgres
{
    /// <summary>
    /// Provides extension methods for PostgreSQL security labels.
    /// </summary>
    /// <remarks>
    /// PostgreSQL security labels allow you to apply access control labels to database objects.
    /// This is commonly used with PostgreSQL security label providers like SELinux (sepgsql)
    /// or PostgreSQL Anonymizer (anon).
    /// <para>
    /// The syntax for security labels in PostgreSQL is:
    /// <code>SECURITY LABEL [FOR provider] ON object_type object_name IS 'label';</code>
    /// </para>
    /// </remarks>
    public static partial class PostgresExtensions
    {
        /// <summary>
        /// Creates a security label on a database object.
        /// </summary>
        /// <param name="context">The migration context.</param>
        /// <returns>A builder for specifying the security label details.</returns>
        /// <example>
        /// <code>
        /// // Create a security label on a table
        /// this.CreateSecurityLabel()
        ///     .OnTable("users")
        ///     .InSchema("public")
        ///     .WithProvider("anon")
        ///     .WithLabel("TABLESAMPLE BERNOULLI(10)");
        /// 
        /// // Create a security label on a column
        /// this.CreateSecurityLabel()
        ///     .OnColumn("email")
        ///     .OnTable("users")
        ///     .InSchema("public")
        ///     .WithProvider("anon")
        ///     .WithLabel("MASKED WITH FUNCTION anon.fake_email()");
        /// </code>
        /// </example>
        public static ICreateSecurityLabelOnObjectSyntax CreateSecurityLabel(this IMigrationContext context)
        {
            return new CreateSecurityLabelExpressionBuilder(context);
        }

        /// <summary>
        /// Deletes a security label from a database object by setting it to NULL.
        /// </summary>
        /// <param name="context">The migration context.</param>
        /// <returns>A builder for specifying the security label to delete.</returns>
        /// <example>
        /// <code>
        /// // Delete a security label from a table
        /// this.DeleteSecurityLabel()
        ///     .FromTable("users")
        ///     .InSchema("public")
        ///     .WithProvider("anon");
        /// 
        /// // Delete a security label from a column
        /// this.DeleteSecurityLabel()
        ///     .FromColumn("email")
        ///     .OnTable("users")
        ///     .InSchema("public")
        ///     .WithProvider("anon");
        /// </code>
        /// </example>
        public static IDeleteSecurityLabelFromObjectSyntax DeleteSecurityLabel(this IMigrationContext context)
        {
            return new DeleteSecurityLabelExpressionBuilder(context);
        }
    }
}
