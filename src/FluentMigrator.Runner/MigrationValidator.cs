using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using FluentMigrator.Expressions;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Exceptions;
using FluentMigrator.Runner.Logging;
using FluentMigrator.Validation;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace FluentMigrator.Runner
{
    public class MigrationValidator
    {
        [CanBeNull]
        private readonly ILogger _logger;

        [CanBeNull]
        private readonly IConventionSet _conventions;

        [CanBeNull]
        private readonly IServiceProvider _serviceProvider;

        internal MigrationValidator(
            [NotNull] ILogger logger,
            [NotNull] IConventionSet conventions,
            [NotNull] IServiceProvider serviceProvider)
        {
            _logger = logger;
            _conventions = conventions;
            _serviceProvider = serviceProvider;
        }

        // ReSharper disable once UnusedMember.Global
        public MigrationValidator(
            [NotNull] ILogger<MigrationValidator> logger,
            [NotNull] IConventionSet conventions,
            [NotNull] IServiceProvider serviceProvider)
        {
            _logger = logger;
            _conventions = conventions;
            _serviceProvider = serviceProvider;
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
                var validationErrors = new Collection<ValidationResult>();
                if (!ValidateExpression(expression, validationErrors, _serviceProvider))
                {
                    foreach (var result in validationErrors.Where(r => r != ValidationResult.Success))
                    {
                        errors.Add(result.ErrorMessage);
                    }
                }

                if (errors.Count > 0)
                {
                    AppendError(errorMessageBuilder, expression.GetType().Name, string.Join(" ", errors.ToArray()));
                }
            }

            if (errorMessageBuilder.Length > 0)
            {
                var errorMessage = errorMessageBuilder.ToString();
                _logger?.LogError("The migration {0} contained the following Validation Error(s): {1}", migration.GetType().Name, errorMessage);
                throw new InvalidMigrationException(migration, errorMessage);
            }
        }

        private static bool ValidateExpression(IMigrationExpression expression, ICollection<ValidationResult> validationResults, IServiceProvider serviceProvider)
        {
            var items = new Dictionary<object, object>();

            var context = new ValidationContext(expression, items);
            if (serviceProvider != null)
            {
                context.InitializeServiceProvider(serviceProvider.GetService);
            }

            return ValidationUtilities.TryCollectResults(context, expression, validationResults);
        }

        private void AppendError(StringBuilder builder, string expressionType, string errors)
        {
            builder.AppendFormat("{0}: {1}{2}", expressionType, errors, Environment.NewLine);
        }
    }
}
