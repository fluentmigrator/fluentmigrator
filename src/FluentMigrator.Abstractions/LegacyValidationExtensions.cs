#region License
// Copyright (c) 2018, FluentMigrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using FluentMigrator.Infrastructure;

namespace FluentMigrator
{
    /// <summary>
    /// Extension methods for the obsolete <see cref="ICanBeValidated"/>
    /// </summary>
    [Obsolete]
    public static class LegacyValidationExtensions
    {
        /// <summary>
        /// Collects all error messages
        /// </summary>
        /// <param name="value">The value to collect the errors for</param>
        /// <param name="errors">The collected errors</param>
        /// <param name="serviceProvider">The service provider used to resolve services</param>
        [Obsolete]
        public static void CollectErrors(this ICanBeValidated value, ICollection<string> errors, IServiceProvider serviceProvider = null)
        {
            var results = new List<ValidationResult>();
            if (!value.TryCollectResults(results, serviceProvider))
            {
                foreach (var result in results.Where(r => r != ValidationResult.Success))
                {
                    errors.Add(result.ErrorMessage);
                }
            }
        }

        /// <summary>
        /// Tries to collect errors using the data annotation validation
        /// </summary>
        /// <param name="rootValue">The value to collect the errors for</param>
        /// <param name="results">The validation results</param>
        /// <param name="serviceProvider">The service provider used to resolve services</param>
        /// <param name="items">Key/value pairs passed to the validation functions</param>
        /// <returns><c>true</c> when no errors were found</returns>
        [Obsolete]
        public static bool TryCollectResults(this ICanBeValidated rootValue, ICollection<ValidationResult> results, IServiceProvider serviceProvider, IDictionary<object, object> items = null)
        {
            var context = new ValidationContext(rootValue, items);
            if (serviceProvider != null)
            {
                context.InitializeServiceProvider(serviceProvider.GetService);
            }

            return ValidationUtilities.TryCollectResults(context, rootValue, results);
        }
    }
}
