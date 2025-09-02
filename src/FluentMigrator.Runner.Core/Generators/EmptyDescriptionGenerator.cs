using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Expressions;

namespace FluentMigrator.Runner.Generators
{
    /// <summary>
    /// A no-op implementation of <see cref="IDescriptionGenerator"/> that generates empty or default description statements.
    /// </summary>
    /// <remarks>
    /// This class is used in scenarios where description generation is not required or supported.
    /// It returns empty strings or empty collections for all description generation methods.
    /// </remarks>
    public class EmptyDescriptionGenerator : IDescriptionGenerator
    {
        /// <inheritdoc />
        public IEnumerable<string> GenerateDescriptionStatements(CreateTableExpression expression)
        {
            return Enumerable.Empty<string>();
        }

        /// <inheritdoc />
        public string GenerateDescriptionStatement(AlterTableExpression expression)
        {
            return string.Empty;
        }

        /// <inheritdoc />
        public string GenerateDescriptionStatement(CreateColumnExpression expression)
        {
            return string.Empty;
        }

        /// <inheritdoc />
        public string GenerateDescriptionStatement(AlterColumnExpression expression)
        {
            return string.Empty;
        }
    }
}
