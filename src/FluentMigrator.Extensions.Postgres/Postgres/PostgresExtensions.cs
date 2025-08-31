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

        public const string IncludesList = "PostgresIncludes";
        public const string IndexAlgorithm = "PostgresIndexAlgorithm";
        public const string Concurrently = "PostgresConcurrently";
        public const string Only = "PostgresOnly";

        public const string IndexFillFactor = "PostgresFillFactor";
        public const string IndexVacuumCleanupIndexScaleFactor = "PostgresBTreeVacuumCleanupIndexScaleFactor";
        public const string IndexFastUpdate = "PostgresGinFastUpdate";
        public const string IndexGinPendingListLimit = "PostgresGinPendingListLimit";
        public const string IndexBuffering = "PostgresGiSTBuffering";
        public const string IndexPagesPerRange = "PostgresBrinPagesPerRange";
        public const string IndexAutosummarize = "PostgresBrinautosummarize";
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
        /// between the the return types of AddColumn or AlterColumn on the IAlterTableColumnAsTypeSyntax interface which is inconvenient
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
        /// between the the return types of AddColumn or AlterColumn on the IAlterTableColumnAsTypeSyntax interface which is inconvenient
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
        /// between the the return types of AddColumn or AlterColumn on the IAlterTableColumnAsTypeSyntax interface which is inconvenient
        /// but helps prevent misuse.
        /// </remarks>
        public static IAlterColumnOptionSyntax SetIdentity(this IAlterColumnOptionSyntax expression, PostgresGenerationType generation)
            => SetIdentity(expression, generation, PostgresIdentityModificationType.Set, GetColumn(expression));

        private static ISupportAdditionalFeatures GetColumn<TNext, TNextFk>(IColumnOptionSyntax<TNext, TNextFk> expression)
            where TNext : IFluentSyntax
            where TNextFk : IFluentSyntax
        {
            if (expression is IColumnExpressionBuilder cast)
            {
                return (ISupportAdditionalFeatures)(object)cast.Column;
            }
            throw new InvalidOperationException(UnsupportedMethodMessage("IdentityGeneration", nameof(IColumnExpressionBuilder)));
        }

        private static TNext SetIdentity<TNext, TNextFk>(IColumnOptionSyntax<TNext, TNextFk> expression, PostgresGenerationType? generation, PostgresIdentityModificationType modificationType, ISupportAdditionalFeatures castColumn)
            where TNext : IFluentSyntax
            where TNextFk : IFluentSyntax
        {
            if (generation.HasValue)
                castColumn.AdditionalFeatures[IdentityGeneration] = generation;

            castColumn.AdditionalFeatures[IdentityModificationType] = modificationType;

            return expression.Identity();
        }

        private static string UnsupportedMethodMessage(string methodName, string interfaceName)
            => string.Format(ErrorMessages.MethodXMustBeCalledOnObjectImplementingY, methodName, interfaceName);

    }
}
