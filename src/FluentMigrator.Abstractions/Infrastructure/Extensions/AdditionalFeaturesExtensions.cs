#region License
//
// Copyright (c) 2018, Fluent Migrator Project
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;

namespace FluentMigrator.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="ISupportAdditionalFeatures"/> interface
    /// </summary>
    public static class AdditionalFeaturesExtensions
    {
        /// <summary>
        /// Gets an additional feature value
        /// </summary>
        /// <typeparam name="T">The value type</typeparam>
        /// <param name="additionalFeatures">The additional feature values</param>
        /// <param name="key">The key into the <see cref="ISupportAdditionalFeatures.AdditionalFeatures"/> dictionary</param>
        /// <param name="defaultValue">The default value to be used if none was found</param>
        /// <returns>The stored value or the <paramref name="defaultValue"/></returns>
        public static T GetAdditionalFeature<T>(this ISupportAdditionalFeatures additionalFeatures, string key, T defaultValue = default)
        {
            T result;
            var dict = additionalFeatures.AdditionalFeatures;
            if (!dict.TryGetValue(key, out var val))
            {
                result = defaultValue;
                dict.Add(key, result);
            }
            else
            {
                result = (T)val;
            }

            return result;
        }

        /// <summary>
        /// Gets an additional feature value
        /// </summary>
        /// <typeparam name="T">The value type</typeparam>
        /// <param name="additionalFeatures">The additional feature values</param>
        /// <param name="key">The key into the <see cref="ISupportAdditionalFeatures.AdditionalFeatures"/> dictionary</param>
        /// <param name="createDefaultValue">A lambda to create a default value</param>
        /// <returns>The stored or a newly created value</returns>
        public static T GetAdditionalFeature<T>(this ISupportAdditionalFeatures additionalFeatures, string key, Func<T> createDefaultValue)
        {
            T result;
            var dict = additionalFeatures.AdditionalFeatures;
            if (!dict.TryGetValue(key, out var val))
            {
                result = createDefaultValue();
                dict.Add(key, result);
            }
            else
            {
                result = (T)val;
            }

            return result;
        }

        /// <summary>
        /// Sets the value for an additional feature
        /// </summary>
        /// <typeparam name="T">The value type</typeparam>
        /// <param name="additionalFeatures">The additional feature values</param>
        /// <param name="key">The key into the <see cref="ISupportAdditionalFeatures.AdditionalFeatures"/> dictionary</param>
        /// <param name="value">The value to be set</param>
        public static void SetAdditionalFeature<T>(this ISupportAdditionalFeatures additionalFeatures, string key, T value)
        {
            var dict = additionalFeatures.AdditionalFeatures;
            dict[key] = value;
        }
    }
}
