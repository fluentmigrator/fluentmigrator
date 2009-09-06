using System.Collections.Generic;
using System.ComponentModel;

namespace FluentMigrator.Builders.Insert
{
	public class InsertDataExpressionBuilder : IInsertDataSyntax
	{
		private readonly InsertDataExpression expression;

		public InsertDataExpressionBuilder(InsertDataExpression expression)
		{
			this.expression = expression;
		}

		public IInsertDataSyntax Row(object dataAsAnonymousType)
		{
			expression.Rows.Add(GetData(dataAsAnonymousType));
			return this;
		}

		private InsertionData GetData(object dataAsAnonymousType)
		{
			var data = new InsertionData();
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(dataAsAnonymousType);
			
			foreach (PropertyDescriptor property in properties)
			{				
				data.Add(new KeyValuePair<string, object>(property.Name, property.GetValue(dataAsAnonymousType)));
			}
			return data;
		}
	}
}