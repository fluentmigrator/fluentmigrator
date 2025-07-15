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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using FluentMigrator.Expressions;

using JetBrains.Annotations;

namespace FluentMigrator.Validation
{
    /// <summary>
    /// Default implementation of a <see cref="IMigrationExpressionValidator"/>
    /// </summary>
    public class DefaultMigrationExpressionValidator : IMigrationExpressionValidator
    {
        [CanBeNull]
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMigrationExpressionValidator"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        public DefaultMigrationExpressionValidator([CanBeNull] IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public virtual IEnumerable<ValidationResult> Validate(IMigrationExpression expression)
        {
#if NET
            if (!System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
            {
                return Enumerable.Empty<ValidationResult>();
            }
#endif

            var items = new Dictionary<object, object>();
            var context = new ValidationContext(expression, items);
            if (_serviceProvider != null)
            {
                context.InitializeServiceProvider(_serviceProvider.GetService);
            }

            var result = new List<ValidationResult>();
            if (!ValidationUtilities.TryCollectResults(context, expression, result))
            {
                return result;
            }

            return Enumerable.Empty<ValidationResult>();
        }
    }
}
