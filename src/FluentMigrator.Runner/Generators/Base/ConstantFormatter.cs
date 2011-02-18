

namespace FluentMigrator.Runner.Generators.Base
{
    using System;

	public class ConstantFormatter : IConstantFormatter
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
            if(value.GetType().IsEnum)
            {
                return "'" + value.ToString() + "'";
            }


			return value.ToString();
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
