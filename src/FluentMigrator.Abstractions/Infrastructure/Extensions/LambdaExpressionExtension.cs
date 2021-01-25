using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentMigrator.Infrastructure.Extensions
{
    public static class LambdaExpressionExtension
    {
        public static string GetMemberName(this LambdaExpression memberSelector)
        {
            var currentExpression = memberSelector.Body;

            while (true)
            {
                switch (currentExpression.NodeType)
                {
                    case ExpressionType.Parameter:
                        return ((ParameterExpression)currentExpression).Name;
                    case ExpressionType.MemberAccess:
                        return GetMeberNameWithColumnAttribute(((MemberExpression)currentExpression).Member);
                    case ExpressionType.Call:
                        return ((MethodCallExpression)currentExpression).Method.Name;
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                        currentExpression = ((UnaryExpression)currentExpression).Operand;
                        break;
                    case ExpressionType.Invoke:
                        currentExpression = ((InvocationExpression)currentExpression).Expression;
                        break;
                    case ExpressionType.ArrayLength:
                        return "Length";
                    default:
                        throw new Exception("not a proper member selector");
                }
            }
        }

        public static string GetMeberNameWithColumnAttribute(MemberInfo memberInfo)
        {
            var columnAttribute = memberInfo.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;

            if (columnAttribute != null)
                return columnAttribute.Name;

            return memberInfo.Name;
        }
    }
}
