using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator.Builders.Schema.Column;

namespace FluentMigrator.Builders.Schema.Table
{
	public interface ISchemaTableSyntax
	{
		bool Exists();
		ISchemaColumnSyntax Column(string column);
	}
}