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

using FluentMigrator.Builders;
using FluentMigrator.Builders.Alter.Column;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Postgres
{
    /// <summary>
    /// Feature extensions for PostgreSQL.
    /// </summary>
    /// <remarks>
    /// <para>Given:</para>
    /// <code>
    /// MigrationBase m = null;
    /// </code>
    /// <para>These are valid calls:</para>
    /// <code>
    /// m.Alter.Column("").OnTable("").AsInt16().AddIdentity(PostgresGenerationType.Always);
    /// m.Alter.Column("").OnTable("").AsInt16().SetIdentity(PostgresGenerationType.Always);
    /// m.Alter.Column("").OnTable("").AsInt16().DropIdentity(true);
    /// m.Alter.Column("").OnTable("").AsInt16().Identity();
    /// m.Alter.Column("").OnTable("").AsInt16().Identity(PostgresGenerationType.Always);  //Ideally would like to stop this, forcing use of AddIdentity instead, but can't
    /// m.Alter.Table("").AddColumn("").AsInt16().Identity(PostgresGenerationType.Always);
    /// m.Alter.Table("").AlterColumn("").AsInt16().Identity(PostgresGenerationType.Always);
    /// </code>
    /// <para>These are not possible:</para>
    /// <code>
    /// m.Alter.Table("").AddColumn("").AsInt16().AddIdentity(PostgresGenerationType.Always);
    /// m.Alter.Table("").AddColumn("").AsInt16().SetIdentity(PostgresGenerationType.Always);
    /// m.Alter.Table("").AddColumn("").AsInt16().DropIdentity(PostgresGenerationType.Always);
    /// m.Alter.Table("").AlterColumn("").AsInt16().AddIdentity(PostgresGenerationType.Always);  //Ideally would like to have these 3, but can't distinguish between return type AddColumn and AlterColumn at compiletime
    /// m.Alter.Table("").AlterColumn("").AsInt16().SetIdentity(PostgresGenerationType.Always);
    /// m.Alter.Table("").AlterColumn("").AsInt16().DropIdentity(false);
    /// </code>
    /// </remarks>
    public static partial class PostgresExtensions
    {
        /// <summary>
        /// Specifies the list of included columns for a PostgreSQL index.  This is a comma-separated list of column names.
        /// </summary>
        public const string IncludesList = "PostgresIncludes";
        /// <summary>
        /// Specifies the index algorithm to use for a PostgreSQL index.  Valid values are "btree", "hash", "gist", "gin", "spgist" or "brin".
        /// </summary>
        public const string IndexAlgorithm = "PostgresIndexAlgorithm";
        /// <summary>
        /// Specifies that the operation should be performed concurrently.  Only valid for Create.Index and Drop.Index operations.
        /// </summary>
        public const string Concurrently = "PostgresConcurrently";
        /// <summary>
        /// Specifies that the operation is only supported on PostgreSQL.  This is used to prevent running a migration on an unsupported database.
        /// </summary>
        public const string Only = "PostgresOnly";

        /// <summary>
        /// Specifies the fill factor for a PostgreSQL index.  Valid values are between 10 and 100 (inclusive).
        /// </summary>
        public const string IndexFillFactor = "PostgresFillFactor";
        /// <summary>
        /// Specifies the minimum number of index pages that must be visited before a bitmap scan is considered.  Valid values are between 0 and 2147483647 (inclusive).
        /// </summary>
        public const string IndexVacuumCleanupIndexScaleFactor = "PostgresBTreeVacuumCleanupIndexScaleFactor";
        /// <summary>
        /// Specifies whether to use fast update for a GIN index.  Valid values are true or false.
        /// </summary>
        public const string IndexFastUpdate = "PostgresGinFastUpdate";
        /// <summary>
        /// Specifies the maximum number of entries in the pending list for a GIN index.  Valid values are between 0 and 2147483647 (inclusive).
        /// </summary>
        public const string IndexGinPendingListLimit = "PostgresGinPendingListLimit";
        /// <summary>
        /// Specifies the amount of buffering to use when building a GiST index.  Valid values are between 0 and 100 (inclusive).
        /// </summary>
        public const string IndexBuffering = "PostgresGiSTBuffering";
        /// <summary>
        /// Specifies the number of heap pages per range for a BRIN index.  Valid values are between 1 and 2147483647 (inclusive).
        /// </summary>
        public const string IndexPagesPerRange = "PostgresBrinPagesPerRange";
        /// <summary>
        /// Specifies whether to automatically summarize ranges for a BRIN index.  Valid values are true or false.
        /// </summary>
        public const string IndexAutosummarize = "PostgresBrinautosummarize";
        /// <summary>
        /// Specifies how NULLs are treated in a PostgreSQL index.  Valid values are "NULLS FIRST", "NULLS LAST" or null (default).
        /// </summary>
        public const string IndexColumnNullsDistinct = "PostgresIndexColumnNullsDistinct";

        /// <summary>
        /// Column identity generation ability for PostgreSQL 10 and above
        /// </summary>
        public static string IdentityGeneration => "PostgresIdentityGeneration";
        /// <summary>
        /// Column identity modification type for PostgreSQL 10 and above
        /// </summary>
        public static string IdentityModificationType => "PostgresIdentityModificationType";

        /// <summary>
        /// Sets the column's identity generation attribute.  To change or remove an existing one, use Alter.Column instead of Alter.Table.AlterColumn
        /// </summary>
        /// <typeparam name="TNext"></typeparam>
        /// <typeparam name="TNextFk"></typeparam>
        /// <param name="expression"></param>
        /// <param name="generation"></param>
        /// <returns>The next step</returns>
        public static TNext Identity<TNext, TNextFk>(this IColumnOptionSyntax<TNext, TNextFk> expression, PostgresGenerationType generation)
            where TNext : IFluentSyntax
            where TNextFk : IFluentSyntax
            => SetIdentity(expression, generation, PostgresIdentityModificationType.Add, GetColumn(expression));

        /// <summary>
        /// Drops an existing identity on the column
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="ifExists">If true and the column is not an identity column, no error is thrown.  In this case a notice is issued instead.</param>
        /// <returns>The next step</returns>
        /// <remarks>Deliberate choice to extend IAlterColumnOptionSyntax rather than IColumnOptionSyntax&lt;TNext, TNextFk&gt;
        /// in order to prevent using these methods when adding a column to the table, since it makes no sense.  It does mean
        /// the syntax migration.Alter.Table("tableName").AlterColumn("columnName") cannot be used since no distinction is made
        /// between the return types of AddColumn or AlterColumn on the IAlterTableColumnAsTypeSyntax interface which is inconvenient
        /// but helps prevent misuse.
        /// </remarks>
        public static IAlterColumnOptionSyntax DropIdentity(this IAlterColumnOptionSyntax expression, bool ifExists)
            => SetIdentity(expression, null, ifExists ? PostgresIdentityModificationType.DropIfExists : PostgresIdentityModificationType.Drop, GetColumn(expression));

        /// <summary>
        /// Adds a generated identity to the column
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="generation"></param>
        /// <returns>The next step</returns>
        /// <remarks>This is an equivalent to Alter.Table.AlterColumn.Identity(PostgresGenerationType)
        /// Deliberate choice to extend IAlterColumnOptionSyntax rather than IColumnOptionSyntax&lt;TNext, TNextFk&gt;
        /// in order to prevent using these methods when adding a column to the table, since it makes no sense.  It does mean
        /// the syntax migration.Alter.Table("tableName").AlterColumn("columnName") cannot be used since no distinction is made
        /// between the return types of AddColumn or AlterColumn on the IAlterTableColumnAsTypeSyntax interface which is inconvenient
        /// but helps prevent misuse.
        /// </remarks>
        public static IAlterColumnOptionSyntax AddIdentity(this IAlterColumnOptionSyntax expression, PostgresGenerationType generation)
            => SetIdentity(expression, generation, PostgresIdentityModificationType.Add, GetColumn(expression));

        /// <summary>
        /// Alters the strategy for an existing generated identity on the column
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="generation"></param>
        /// <returns>The next step</returns>
        /// <remarks>Deliberate choice to extend IAlterColumnOptionSyntax rather than IColumnOptionSyntax&lt;TNext, TNextFk&gt;
        /// in order to prevent using these methods when adding a column to the table, since it makes no sense.  It does mean
        /// the syntax migration.Alter.Table("tableName").AlterColumn("columnName") cannot be used since no distinction is made
        /// between the return types of AddColumn or AlterColumn on the IAlterTableColumnAsTypeSyntax interface which is inconvenient
        /// but helps prevent misuse.
        /// </remarks>
        public static IAlterColumnOptionSyntax SetIdentity(this IAlterColumnOptionSyntax expression, PostgresGenerationType generation)
            => SetIdentity(expression, generation, PostgresIdentityModificationType.Set, GetColumn(expression));

        /// <summary>
        /// Retrieves the column associated with the specified column option syntax expression.
        /// </summary>
        /// <typeparam name="TNext">The type of the next fluent syntax in the chain.</typeparam>
        /// <typeparam name="TNextFk">The type of the next fluent syntax for foreign key operations in the chain.</typeparam>
        /// <param name="expression">The column option syntax expression from which the column is to be retrieved.</param>
        /// <returns>An object that supports additional features for the column.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the provided <paramref name="expression"/> does not implement <see cref="IColumnExpressionBuilder"/>.
        /// </exception>
        private static ISupportAdditionalFeatures GetColumn<TNext, TNextFk>(IColumnOptionSyntax<TNext, TNextFk> expression)
            where TNext : IFluentSyntax
            where TNextFk : IFluentSyntax
        {
            if (expression is IColumnExpressionBuilder cast)
            {
                return cast.Column;
            }
            throw new InvalidOperationException(UnsupportedMethodMessage("IdentityGeneration", nameof(IColumnExpressionBuilder)));
        }

        /// <summary>
        /// Configures the identity generation and modification type for a PostgreSQL column.
        /// </summary>
        /// <typeparam name="TNext">The next fluent syntax type in the chain.</typeparam>
        /// <typeparam name="TNextFk">The next fluent syntax type for foreign key operations in the chain.</typeparam>
        /// <param name="expression">The column option syntax to configure.</param>
        /// <param name="generation">The type of identity generation to apply, or <c>null</c> if no generation type is specified.</param>
        /// <param name="modificationType">The type of identity modification to apply.</param>
        /// <param name="castColumn">The column supporting additional features.</param>
        /// <returns>The next fluent syntax in the chain.</returns>
        /// <remarks>
        /// This method modifies the <paramref name="castColumn"/> by adding or updating its additional features
        /// with the specified identity generation and modification type.
        /// </remarks>
        private static TNext SetIdentity<TNext, TNextFk>(IColumnOptionSyntax<TNext, TNextFk> expression, PostgresGenerationType? generation, PostgresIdentityModificationType modificationType, ISupportAdditionalFeatures castColumn)
            where TNext : IFluentSyntax
            where TNextFk : IFluentSyntax
        {
            if (generation.HasValue)
                castColumn.AdditionalFeatures[IdentityGeneration] = generation;

            castColumn.AdditionalFeatures[IdentityModificationType] = modificationType;

            return expression.Identity();
        }

        /// <summary>
        /// Generates an error message indicating that a specific method must be called on an object implementing a specific interface.
        /// </summary>
        /// <param name="methodName">The name of the method that was called incorrectly.</param>
        /// <param name="interfaceName">The name of the required interface that the object must implement.</param>
        /// <returns>A formatted error message describing the issue.</returns>
        private static string UnsupportedMethodMessage(string methodName, string interfaceName)
            => string.Format(ErrorMessages.MethodXMustBeCalledOnObjectImplementingY, methodName, interfaceName);

    }
}
