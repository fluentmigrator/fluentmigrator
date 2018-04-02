using System.Collections.Generic;
using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Generators
{
    /// <summary>
    /// Generate SQL statements to set descriptions for tables and columns
    /// </summary>
    public interface IDescriptionGenerator
    {
        IEnumerable<string> GenerateDescriptionStatements(CreateTableExpression expression);
        string GenerateDescriptionStatement(AlterTableExpression expression);
        string GenerateDescriptionStatement(CreateColumnExpression expression);
        string GenerateDescriptionStatement(AlterColumnExpression expression);
    }
}
