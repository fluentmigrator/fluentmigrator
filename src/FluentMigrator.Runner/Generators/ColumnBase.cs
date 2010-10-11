using System.Data;
using FluentMigrator.Model;

namespace FluentMigrator.Runner.Generators
{
	internal abstract class ColumnBase : IColumn
	{
		private readonly ITypeMap _typeMap;
		private readonly IConstantFormatter _constantFormatter;

		public ColumnBase(ITypeMap typeMap, IConstantFormatter constantFormatter)
		{			_typeMap = typeMap;
			_constantFormatter = constantFormatter;
		}

		protected string GetTypeMap(DbType value, int size, int precision)
		{
			return _typeMap.GetTypeMap(value, size, precision);
		}

		protected IConstantFormatter Constant
		{
			get { return _constantFormatter; }
		}

		public abstract string Generate(ColumnDefinition definition);
	}
}