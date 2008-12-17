using System;
using System.Data;

namespace FluentMigrator.Model
{
	public class ColumnDefinition
	{
		public string Name { get; set; }
		public DbType Type { get; set; }
		public int Size { get; set; }
		public object DefaultValue { get; set; }
		public bool IsForeignKey { get; set; }
		public bool IsIdentity { get; set; }
		public bool IsIndexed { get; set; }
		public bool IsPrimaryKey { get; set; }
		public bool IsNullable { get; set; }
		public bool IsUnique { get; set; }

		public ColumnDefinition(string name)
		{
			Name = name;
		}
	}
}