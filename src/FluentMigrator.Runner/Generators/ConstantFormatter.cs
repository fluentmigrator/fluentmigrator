using System;

namespace FluentMigrator.Runner.Generators
{
	public class ConstantFormatter
	{
		public virtual string Format(object value)
		{
			if (value == null)
			{
				return "NULL";
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
				return FormatBool(value);
			}
			if (value is Guid)
			{
				return "'" + ((Guid)value).ToString().Replace("'", "''") + "'";
			}
			if (value is DateTime)
			{
				return "'" + ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss") + "'";
			}
			if (value.GetType().IsEnum)
			{
				return "'" + value + "'";
			}

			return value.ToString();
		}

		protected virtual string FormatBool(object value)
		{
			return ((bool)value) ? 1.ToString() : 0.ToString();
		}
	}

	public class PostgresFormatter : ConstantFormatter
	{
		protected override string FormatBool(object value)
		{
			return ((bool)value) ? "true" : "false";
		}
	}

	public class ConstantFormatterWithQuotedBackslashes : ConstantFormatter
	{
		public override string Format(object value)
		{
			return base.Format(value).Replace(@"\", @"\\");
		}
	}
}
