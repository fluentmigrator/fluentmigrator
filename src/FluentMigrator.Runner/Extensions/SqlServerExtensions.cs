using System;
using FluentMigrator.Builders.Insert;

namespace FluentMigrator.Runner.Extensions
{
    public static class SqlServerExtensions
    {
        public const string IdentityInsert = "SqlServerIdentityInsert";

        /// <summary>
        /// Inserts data using Sql Server's IDENTITY INSERT feature.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IInsertDataSyntax WithIdentityInsert(this IInsertDataSyntax expression)
        {
            IInsertDataAdditionalFeatures castExpression = expression as IInsertDataAdditionalFeatures;
            if (castExpression == null)
            {
                throw new InvalidOperationException("WithIdentityInsert must be called on an object that implements IInsertDataAdditionalFeatures.");
            }
            castExpression.AddAdditionalFeature(IdentityInsert, true);
            return expression;
        }
    }
}
