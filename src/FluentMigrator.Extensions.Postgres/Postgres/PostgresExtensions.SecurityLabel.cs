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

using FluentMigrator.Builders.Create;
using FluentMigrator.Builders.Delete;
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
        /// <param name="root">The create expression root.</param>
        /// <returns>A builder for specifying the security label details.</returns>
        /// <example>
        /// <code>
        /// // Create a security label on a table
        /// Create.SecurityLabel()
        ///     .OnTable("users")
        ///     .InSchema("public")
        ///     .WithLabel("some label");
        /// 
        /// // Create a security label on a table with a specific provider
        /// Create.SecurityLabel()
        ///     .For("anon")
        ///     .OnTable("users")
        ///     .InSchema("public")
        ///     .WithLabel("TABLESAMPLE BERNOULLI(10)");
        /// 
        /// // Create a security label on a column
        /// Create.SecurityLabel()
        ///     .For("anon")
        ///     .OnColumn("email")
        ///     .OnTable("users")
        ///     .InSchema("public")
        ///     .WithLabel("MASKED WITH FUNCTION anon.fake_email()");
        /// </code>
        /// </example>
        public static ICreateSecurityLabelSyntax SecurityLabel(this ICreateExpressionRoot root)
        {
            var context = GetMigrationContext(root);
            return new CreateSecurityLabelExpressionBuilder(context);
        }

        /// <summary>
        /// Deletes a security label from a database object by setting it to NULL.
        /// </summary>
        /// <param name="root">The delete expression root.</param>
        /// <returns>A builder for specifying the security label to delete.</returns>
        /// <example>
        /// <code>
        /// // Delete a security label from a table
        /// Delete.SecurityLabel()
        ///     .For("anon")
        ///     .FromTable("users")
        ///     .InSchema("public");
        /// 
        /// // Delete a security label from a column
        /// Delete.SecurityLabel()
        ///     .For("anon")
        ///     .FromColumn("email")
        ///     .OnTable("users")
        ///     .InSchema("public");
        /// </code>
        /// </example>
        public static IDeleteSecurityLabelSyntax SecurityLabel(this IDeleteExpressionRoot root)
        {
            var context = GetMigrationContext(root);
            return new DeleteSecurityLabelExpressionBuilder(context);
        }

        /// <summary>
        /// Gets the migration context from a Create or Delete expression root.
        /// </summary>
        /// <param name="root">The expression root (CreateExpressionRoot or DeleteExpressionRoot).</param>
        /// <returns>The migration context.</returns>
        /// <remarks>
        /// This uses reflection to access the private _context field because the 
        /// ICreateExpressionRoot and IDeleteExpressionRoot interfaces don't expose the context.
        /// This is necessary for adding new expression types that need to be added to the
        /// context's expression list.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the context cannot be accessed from the expression root.
        /// </exception>
        private static IMigrationContext GetMigrationContext(object root)
        {
            var rootType = root.GetType();
            var contextField = rootType.GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (contextField is null)
            {
                throw new System.InvalidOperationException(
                    $"Cannot access migration context from expression root of type '{rootType.Name}'. " +
                    "SecurityLabel extension methods require CreateExpressionRoot or DeleteExpressionRoot.");
            }
            return (IMigrationContext)contextField.GetValue(root);
        }
    }
}
