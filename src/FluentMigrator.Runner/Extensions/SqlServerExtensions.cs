using System;
using FluentMigrator.Builders;
using FluentMigrator.Builders.Alter.Column;
using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Infrastructure;

namespace FluentMigrator.Runner.Extensions
{
    public static class SqlServerExtensions
    {
        public const string IdentityInsert = "SqlServerIdentityInsert";
        public const string IdentitySeed = "SqlServerIdentitySeed";
        public const string IdentityIncrement = "SqlServerIdentityIncrement";

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
            castExpression.AddAdditionalFeature(IdentityInsert, true);
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
            castColumn.AddAdditionalFeature(IdentitySeed, seed);
            castColumn.AddAdditionalFeature(IdentityIncrement, increment);
            return expression.Identity();
        }

        private static ISupportAdditionalFeatures GetColumn<TNext, TNextFk>(IColumnOptionSyntax<TNext, TNextFk> expression) where TNext : IFluentSyntax where TNextFk : IFluentSyntax 
        {
            CreateTableExpressionBuilder cast1 = expression as CreateTableExpressionBuilder;
            if (cast1 != null) return cast1.CurrentColumn;

            AlterTableExpressionBuilder cast2 = expression as AlterTableExpressionBuilder;
            if (cast2 != null) return cast2.CurrentColumn;

            AlterColumnExpressionBuilder cast3 = expression as AlterColumnExpressionBuilder;
            if (cast3 != null) return cast3.GetColumnForType();

            CreateColumnExpressionBuilder cast4 = expression as CreateColumnExpressionBuilder;
            if (cast4 != null) return cast4.GetColumnForType();

            throw new InvalidOperationException("The seeded identity method can only be called on a valid object.");
        }
    }
}
