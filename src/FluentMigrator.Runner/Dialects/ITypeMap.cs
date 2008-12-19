using System;
using System.Data;

namespace FluentMigrator.Runner.Dialects
{
	public interface ITypeMap
	{
		void Set(DbType type, string template);
		void Set(DbType type, int maxSize, string template);
		string Get(DbType type, int size, int precision, int scale);
	}
}