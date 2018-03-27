using System;
using System.Collections.Generic;

using FluentMigrator.Builders;
using FluentMigrator.Builders.Create.Constraint;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Infrastructure;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;

namespace FluentMigrator
{
    public enum SqlServerConstraintType
    {
        Clustered,
        NonClustered
    }

    public static class SqlServerExtensions
    {
        public const string IdentityInsert = "SqlServerIdentityInsert";
        public const string IdentitySeed = "SqlServerIdentitySeed";
        public const string IdentityIncrement = "SqlServerIdentityIncrement";
        public const string ConstraintType = "SqlServerConstraintType";
        public const string IncludesList = "SqlServerIncludes";
        public const string OnlineIndex = "SqlServerOnlineIndex";
        public const string RowGuidColumn = "SqlServerRowGuidColumn";

        /// <summary>
        /// Inserts data using Sql Server's IDENTITY INSERT feature.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IInsertDataSyntax WithIdentityInsert(this IInsertDataSyntax expression)
        {
            ISupportAdditionalFeatures castExpression = expression as ISupportAdditionalFeatures;
            if (castExpression == null)
            {
                throw new InvalidOperationException("WithIdentityInsert must be called on an object that implements ISupportAdditionalFeatures.");
            }
            castExpression.AdditionalFeatures[IdentityInsert] = true;
            return expression;
        }

        /// <summary>
        /// Makes a column an Identity column using the specified seed and increment values.
        /// </summary>
        /// <param name="expression">Column on which to apply the identity.</param>
        /// <param name="seed">Starting value of the identity.</param>
        /// <param name="increment">Increment value of the identity.</param>
        /// <returns></returns>
        public static TNext Identity<TNext, TNextFk>(this IColumnOptionSyntax<TNext, TNextFk> expression,
            int seed, int increment) where TNext : IFluentSyntax where TNextFk : IFluentSyntax
        {
            ISupportAdditionalFeatures castColumn = GetColumn(expression);
            castColumn.AdditionalFeatures[IdentitySeed] = seed;
            castColumn.AdditionalFeatures[IdentityIncrement] = increment;
            return expression.Identity();
        }

        private static void SetConstraintType(ICreateConstraintOptionsSyntax expression, SqlServerConstraintType type)
        {
            if (!(expression is ISupportAdditionalFeatures additionalFeatures)) throw new InvalidOperationException(type + " must be called on an object that implements ISupportAdditionalFeatures.");

            additionalFeatures.AdditionalFeatures[ConstraintType] = type;
        }

        public static void Clustered(this ICreateConstraintOptionsSyntax expression)
        {
            SetConstraintType(expression, SqlServerConstraintType.Clustered);
        }

        public static void NonClustered(this ICreateConstraintOptionsSyntax expression)
        {
            SetConstraintType(expression, SqlServerConstraintType.NonClustered);
        }

        public static ICreateIndexOptionsSyntax Online(this ICreateIndexOptionsSyntax expression, bool active = true)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures ?? throw new InvalidOperationException("The Online method must be called on an object that implements ISupportAdditionalFeatures.");
            additionalFeatures.AdditionalFeatures[OnlineIndex] = active;
            return expression;
        }

        public static ICreateIndexOptionsSyntax Include(this ICreateIndexOptionsSyntax expression, string columnName)
        {
            var additionalFeatures = expression as ISupportAdditionalFeatures ?? throw new InvalidOperationException("The Include method must be called on an object that implements ISupportAdditionalFeatures.");
            var includes = additionalFeatures.GetAdditionalFeature<IList<IndexIncludeDefinition>>(IncludesList, () => new List<IndexIncludeDefinition>());
            includes.Add(new IndexIncludeDefinition { Name = columnName });
            return expression;
        }

        public static ICreateTableColumnOptionOrWithColumnSyntax RowGuid(this ICreateTableColumnOptionOrWithColumnSyntax expression)
        {
            var columnExpression = expression as IColumnExpressionBuilder ?? throw new InvalidOperationException("The Include method must be called on an object that implements IColumnExpressionBuilder.");
            columnExpression.Column.AdditionalFeatures[RowGuidColumn] = true;
            return expression;
        }

        private static ISupportAdditionalFeatures GetColumn<TNext, TNextFk>(IColumnOptionSyntax<TNext, TNextFk> expression) where TNext : IFluentSyntax where TNextFk : IFluentSyntax
        {
            if (expression is IColumnExpressionBuilder cast1) return cast1.Column;

            throw new InvalidOperationException("The seeded identity method can only be called on a valid object.");
        }

    }
}
