using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using FluentMigrator.Builders.Execute;
using FluentMigrator.Expressions;
using FluentMigrator.Model;
using FluentMigrator.Runner.Generators.Firebird;

namespace FluentMigrator.Runner.Processors.Firebird
{
    public abstract class FirebirdProcessedExpressionBase
    {
        protected Type expressionType;
        public FirebirdProcessor Processor { get; private set; }
        public IMigrationExpression Expression { get; private set; }

        protected FirebirdProcessedExpressionBase(IMigrationExpression expression, Type expressionType, FirebirdProcessor processor)
        {
            Processor = processor;
            Expression = expression;
            this.expressionType = expressionType;
        }

        protected string GenerateSql(IMigrationExpression expression)
        {
            string result = null;
            try
            {
                MethodInfo generatorMethod = Processor.Generator.GetType().GetMethod("Generate", new Type[] { expression.GetType() });
                if (generatorMethod == null)
                    throw new ArgumentOutOfRangeException(String.Format("Can't find generator for {0}", expression.ToString()));

                result = generatorMethod.Invoke(Processor.Generator, new object[] { expression }) as string;
            }
            catch (Exception e)
            {
                throw new ArgumentOutOfRangeException(String.Format("Can't find generator for {0}", expression.ToString()), e);
            }
            return result;
        }

        protected void Run(IMigrationExpression expression, IDbConnection connection, IDbTransaction transaction)
        {
            if (expression is PerformDBOperationExpression)
            {
                (expression as PerformDBOperationExpression).Operation(connection, transaction);
                return;
            }
            string sql = GenerateSql(expression);
            if (String.IsNullOrEmpty(sql))
                return;
            Processor.Announcer.Sql(sql);
            using (var command = Processor.Factory.CreateCommand(sql, connection, transaction, Processor.Options))
            {
                command.ExecuteNonQuery();
            }
        }

        public override string ToString()
        {
            return Expression.ToString();
        }

    }

    public sealed class FirebirdProcessedExpression<T> : FirebirdProcessedExpressionBase where T : IMigrationExpression
    {
        public FirebirdProcessedExpression(T expression, FirebirdProcessor processor)
            : base(expression, typeof(T), processor)
        {
        }
    }

    public sealed class FirebirdProcessedExpression : FirebirdProcessedExpressionBase
    {
        public FirebirdProcessedExpression(IMigrationExpression expression, Type expressionType, FirebirdProcessor processor)
            : base(expression, expressionType, processor)
        {
        }
    }
}
