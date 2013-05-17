using System;
using FluentMigrator.Builders;
using FluentMigrator.Builders.Alter.Column;
using FluentMigrator.Builders.Alter.Table;
using FluentMigrator.Builders.Create.Column;
using FluentMigrator.Builders.Create.Constraint;
using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Builders.Create.Index;
using FluentMigrator.Builders.Insert;
using FluentMigrator.Infrastructure;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Extensions
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

        private static void SetConstraintType(ICreateConstraintOptionsSyntax expression, SqlServerConstraintType type)
        {
            CreateConstraintExpressionBuilder castPrimaryKey = expression as CreateConstraintExpressionBuilder;
            if (castPrimaryKey == null) throw new InvalidOperationException(type + " must be called on an object that implements ISupportAdditionalFeatures.");

            ISupportAdditionalFeatures castExpression = castPrimaryKey.Expression.Constraint;

            castExpression.AddAdditionalFeature(ConstraintType, type);
        }

        public static void Clustered(this ICreateConstraintOptionsSyntax expression)
        {
            SetConstraintType(expression, SqlServerConstraintType.Clustered);
        }

        public static void NonClustered(this ICreateConstraintOptionsSyntax expression)
        {
            SetConstraintType(expression, SqlServerConstraintType.NonClustered);
        }

        public static ICreateIndexOptionsSyntax Include(this ICreateIndexOptionsSyntax expression, string columnName)
        {
            CreateIndexExpressionBuilder castIndex = expression as CreateIndexExpressionBuilder;
            if(castIndex == null) throw new InvalidOperationException("The Include method can only be called on a valid object.");
            castIndex.Expression.Index.Includes.Add(new IndexIncludeDefinition { Name = columnName }); ;
            return expression;
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
