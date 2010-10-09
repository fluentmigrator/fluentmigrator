using System;

namespace FluentMigrator.Runner.Generators
{
	public class ConstantFormatter : IConstantFormatter
	{
		public string Format(object value)
		{
			if (value == null)
			{
				return "null";
			}

			string stringValue = value as string;
			if (stringValue != null)
			{
				return "'" + stringValue.Replace("'", "''") + "'";
			}
			if (value is char)
			{
				return "'" + value + "'";
			}
			if (value is bool)
			{
				return ((bool)value) ? 1.ToString() : 0.ToString();
			}
			if (value is Guid)
			{
				return "'" + ((Guid)value).ToString().Replace("'", "''") + "'";
			}
			if (value is DateTime)
			{
				return "'" + ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss")+ "'";
			}

			return value.ToString();
		}
	}
}
