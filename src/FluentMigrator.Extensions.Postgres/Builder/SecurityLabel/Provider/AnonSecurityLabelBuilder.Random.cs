#region License
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using JetBrains.Annotations;

namespace FluentMigrator.Builder.SecurityLabel.Provider;

/// <summary>
/// RANDOM masking functions for PostgreSQL Anonymizer.
/// These functions generate random values without requiring a seed.
/// </summary>
public partial class AnonSecurityLabelBuilder
{
    /// <summary>
    /// Masks the column with a random string of specified length.
    /// </summary>
    /// <param name="length">The length of the random string. Default is 12. Must be greater than 0.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is less than or equal to 0.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomString(int length = 12)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be greater than 0.");
        }

        return MaskedWithFunction("anon.random_string", length);
    }

    /// <summary>
    /// Masks the column with a random integer.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithRandomInt()
    {
        return MaskedWithFunction("anon.random_int");
    }

    /// <summary>
    /// Masks the column with a random integer within the specified range (inclusive).
    /// </summary>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (inclusive).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomIntBetween(int min, int max)
    {
        if (min > max)
        {
            throw new ArgumentException("Min value cannot be greater than max value.", nameof(min));
        }

        return MaskedWithFunction("anon.random_int_between", min, max);
    }

    /// <summary>
    /// Masks the column with a random date.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithRandomDate()
    {
        return MaskedWithFunction("anon.random_date");
    }

    /// <summary>
    /// Masks the column with a random date within the specified range (inclusive).
    /// </summary>
    /// <param name="startDate">The start date (inclusive) as a SQL-formatted string (e.g., "'2020-01-01'").</param>
    /// <param name="endDate">The end date (inclusive) as a SQL-formatted string (e.g., "'2025-12-31'").</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="startDate"/> or <paramref name="endDate"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithRandomDateBetween([NotNull] string startDate, [NotNull] string endDate)
    {
        if (string.IsNullOrWhiteSpace(startDate))
        {
            throw new ArgumentException("Start date cannot be null or whitespace.", nameof(startDate));
        }

        if (string.IsNullOrWhiteSpace(endDate))
        {
            throw new ArgumentException("End date cannot be null or whitespace.", nameof(endDate));
        }

        return MaskedWithFunction("anon.random_date_between", startDate, endDate);
    }
}

