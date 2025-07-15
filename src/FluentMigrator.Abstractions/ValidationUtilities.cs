#region License
// Copyright (c) 2018, Fluent Migrator Project
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

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using FluentMigrator.Infrastructure;
using FluentMigrator.Validation;

namespace FluentMigrator
{
    /// <summary>
    /// Helper functions to validate objects
    /// </summary>
#if NET
    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode("This type uses reflection to validate migration expressions, which may not be preserved in trimmed applications.")]
#endif
    internal static class ValidationUtilities
    {
        /// <summary>
        /// Collect validation results
        /// </summary>
        /// <param name="context">The validation context</param>
        /// <param name="value">The value to be validated</param>
        /// <param name="results">The collected validation results</param>
        /// <returns><c>true</c> when no errors were found</returns>
        internal static bool TryCollectResults(ValidationContext context, object value, ICollection<ValidationResult> results)
        {
            if (!Validator.TryValidateObject(value, context, results, validateAllProperties: true))
            {
                return false;
            }

            return TryProcessChildren(context, value, results);
        }

        private static bool TryProcessChildren(ValidationContext context, object value, ICollection<ValidationResult> results)
        {
            if (value is IValidationChildren validationChildren)
            {
                foreach (var childValue in validationChildren.Children)
                {
                    if (childValue != null)
                    {
                        var childContext = new ValidationContext(childValue, context.Items);
                        childContext.InitializeServiceProvider(context.GetService);

                        if (!TryCollectResults(childContext, childValue, results))
                        {
                            return false;
                        }
                    }
                }
            }

            if (value is ISupportAdditionalFeatures additionalFeatures)
            {
                foreach (var additionalFeaturesValue in additionalFeatures.AdditionalFeatures.Values)
                {
                    if (additionalFeaturesValue is null)
                    {
                        continue;
                    }

                    var valueType = additionalFeaturesValue.GetType();
                    if (valueType.IsPrimitive)
                    {
                        continue;
                    }

                    switch (additionalFeaturesValue)
                    {
                        case string _:
                        case decimal _:
                            continue;
                    }

                    var childContext = new ValidationContext(additionalFeaturesValue, context.Items);
                    childContext.InitializeServiceProvider(context.GetService);

                    if (!TryCollectResults(childContext, additionalFeaturesValue, results))
                    {
                        return false;
                    }
                }
            }

            if (value is ICollection collection)
            {
                foreach (var item in collection)
                    if (item is IValidatableObject)
                    {
                        var childContext = new ValidationContext(item, context.Items);
                        childContext.InitializeServiceProvider(context.GetService);

                        if (!TryCollectResults(childContext, item, results))
                        {
                            return false;
                        }
                    }
            }

            return true;
        }
    }
}
