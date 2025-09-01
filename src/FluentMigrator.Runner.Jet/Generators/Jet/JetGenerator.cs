using System.Collections.Generic;
using System.Linq;

using FluentMigrator.Expressions;
using FluentMigrator.Runner.Generators.Generic;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

namespace FluentMigrator.Runner.Generators.Jet
{
    /// <summary>
    /// The Jet SQL generator for FluentMigrator.
    /// </summary>
    public class JetGenerator : GenericGenerator
    {
        /// <inheritdoc />
        public JetGenerator()
            : this(new JetQuoter())
        {
        }

        /// <inheritdoc />
        public JetGenerator(
            [NotNull] JetQuoter quoter)
            : this(quoter, new OptionsWrapper<GeneratorOptions>(new GeneratorOptions()))
        {
        }

        /// <inheritdoc />
        public JetGenerator(
            [NotNull] JetQuoter quoter,
            [NotNull] IOptions<GeneratorOptions> generatorOptions)
            : base(new JetColumn(), quoter, new EmptyDescriptionGenerator(), generatorOptions)
        {
        }

        /// <inheritdoc />
        public override string DropIndex => "DROP INDEX {0} ON {1}";

        /// <inheritdoc />
        public override string Generate(RenameTableExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Renaming of tables is not supported for Jet");
        }

        /// <inheritdoc />
        public override string Generate(RenameColumnExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Renaming of columns is not supported for Jet");
        }

        /// <inheritdoc />
        public override string Generate(AlterDefaultConstraintExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Altering of default constraints is not supported for Jet");
        }

        /// <inheritdoc />
        public override string GeneratorId => GeneratorIdConstants.Jet;

        /// <inheritdoc />
        public override List<string> GeneratorIdAliases => new List<string> { GeneratorIdConstants.Jet };

        /// <inheritdoc />
        public override string Generate(DeleteTableExpression expression)
        {
            if (expression.IfExists)
            {
                return CompatibilityMode.HandleCompatibility("If Exists logic is not supported for Jet");
            }
            return base.Generate(expression);
        }

        /// <inheritdoc />
        public override string Generate(CreateSequenceExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Sequences are not supported for Jet");
        }

        /// <inheritdoc />
        public override string Generate(DeleteSequenceExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Sequences are not supported for Jet");
        }

        /// <inheritdoc />
        public override string Generate(DeleteDefaultConstraintExpression expression)
        {
            return CompatibilityMode.HandleCompatibility("Default constraints are not supported");
        }

        /// <inheritdoc />
        public override string Generate(CreateTableExpression expression)
        {
            if (expression.Columns.Any(x => x.Expression != null))
            {
                return CompatibilityMode.HandleCompatibility("Computed columns are not supported for Jet");
            }
            return base.Generate(expression);
        }

        public override string Generate(CreateColumnExpression expression)
        {
            if (expression.Column.Expression != null)
            {
                return CompatibilityMode.HandleCompatibility("Computed columns are not supported for Jet");
            }
            return base.Generate(expression);
        }

        public override string Generate(AlterColumnExpression expression)
        {
            if (expression.Column.Expression != null)
            {
                return CompatibilityMode.HandleCompatibility("Computed columns are not supported for Jet");
            }
            return base.Generate(expression);
        }
    }
}
