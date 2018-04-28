using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Logging;

using Microsoft.Extensions.Logging;

namespace FluentMigrator.Runner
{
    public class MigrationValidator
    {
        private readonly ILogger _logger;
        private readonly IConventionSet _conventions;

        internal MigrationValidator(ILogger logger, IConventionSet conventions)
        {
            _logger = logger;
            _conventions = conventions;
        }

        public MigrationValidator(ILogger<MigrationValidator> logger, IConventionSet conventions)
        {
            _logger = logger;
            _conventions = conventions;
        }

        [Obsolete]
        public MigrationValidator()
        {
        }

        [Obsolete]
        public MigrationValidator(IAnnouncer announcer, IConventionSet conventions)
        {
            _logger = new AnnouncerFluentMigratorLogger(announcer);
            _conventions = conventions;
        }

        /// <summary>
        /// Validates each migration expression that has implemented the ICanBeValidated interface.
        /// It throws an InvalidMigrationException exception if validation fails.
        /// </summary>
        /// <param name="migration">The current migration being run</param>
        /// <param name="expressions">All the expressions contained in the up or down action</param>
        public void ApplyConventionsToAndValidateExpressions(IMigration migration, IEnumerable<IMigrationExpression> expressions)
        {
            var errorMessageBuilder = new StringBuilder();

            foreach (var expression in expressions.Apply(_conventions))
            {
                var errors = new Collection<string>();
                expression.CollectValidationErrors(errors);

                if (errors.Count > 0)
                    AppendError(errorMessageBuilder, expression.GetType().Name, string.Join(" ", errors.ToArray()));
            }

            if (errorMessageBuilder.Length > 0)
            {
                var errorMessage = errorMessageBuilder.ToString();
                _logger?.LogError("The migration {0} contained the following Validation Error(s): {1}", migration.GetType().Name, errorMessage);
                throw new InvalidMigrationException(migration, errorMessage);
            }
        }

        private void AppendError(StringBuilder builder, string expressionType, string errors)
        {
            builder.AppendFormat("{0}: {1}{2}", expressionType, errors, Environment.NewLine);
        }
    }
}
