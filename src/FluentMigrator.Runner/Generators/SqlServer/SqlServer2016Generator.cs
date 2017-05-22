using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators.SqlServer
{
    public class SqlServer2016Generator : SqlServer2014Generator
    {
        public override string Generate(CreateIndexExpression expression)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(base.Generate(expression));

            if (expression.Index.ApplyOnline.HasValue)
            {
                sql.Append(string.Format(" WITH (ONLINE = {0})", (expression.Index.ApplyOnline == OnlineMode.On ? "ON" : "OFF")));
            }

            return sql.ToString();
        }

        public override string Generate(DeleteIndexExpression expression)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(base.Generate(expression));

            if (expression.Index.ApplyOnline.HasValue)
            {
                sql.Append(string.Format(" WITH (ONLINE = {0})", (expression.Index.ApplyOnline == OnlineMode.On ? "ON" : "OFF")));
            }

            return sql.ToString();
        }

        public override string Generate(CreateConstraintExpression expression)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(base.Generate(expression));

            if (expression.Constraint.ApplyOnline.HasValue)
            {
                sql.Append(string.Format(" WITH (ONLINE = {0})", (expression.Constraint.ApplyOnline == OnlineMode.On ? "ON" : "OFF")));
            }

            return sql.ToString();
        }

        public override string Generate(DeleteConstraintExpression expression)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(base.Generate(expression));

            if (expression.Constraint.ApplyOnline.HasValue)
            {
                sql.Append(string.Format(" WITH (ONLINE = {0})", (expression.Constraint.ApplyOnline == OnlineMode.On ? "ON" : "OFF")));
            }

            return sql.ToString();
        }
    }
}
