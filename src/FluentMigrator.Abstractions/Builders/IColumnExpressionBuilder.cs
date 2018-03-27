using FluentMigrator.Model;
using System;

namespace FluentMigrator.Builders
{
   /// <summary>
   /// Describes common attributes for expression builders which have a current table/column.
   /// </summary>
   public interface IColumnExpressionBuilder
   {
      string SchemaName { get; }
      string TableName { get; }
      ColumnDefinition Column { get; }
   }
}
