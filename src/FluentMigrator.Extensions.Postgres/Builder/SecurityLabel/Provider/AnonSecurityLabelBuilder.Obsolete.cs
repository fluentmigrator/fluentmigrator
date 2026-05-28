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
using System.ComponentModel;

namespace FluentMigrator.Builder.SecurityLabel.Provider;

// Backwards-compatibility shims for public methods that shipped in 8.0.1 and were renamed or
// reshaped afterwards. They are restored here with their original signatures and behavior so that
// code compiled against 8.0.1 keeps working when the assembly is swapped without recompilation
// (see package validation in PackageLibrary.props). Hidden from IntelliSense via EditorBrowsable.
public partial class AnonSecurityLabelBuilder
{
    /// <summary>
    /// Masks the column with a fake phone number.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    [Obsolete("Use MaskedWithRandomPhone(string prefix) instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public AnonSecurityLabelBuilder MaskedWithFakePhone()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_phone()");
        return this;
    }

    /// <summary>
    /// Masks the column with a fake SIREN number (French business identifier).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    [Obsolete("anon.fake_siren() is no longer provided by PostgreSQL Anonymizer; use MaskedWithFunction(string, params object[]) for custom functions.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public AnonSecurityLabelBuilder MaskedWithFakeSiren()
    {
        RawLabel("MASKED WITH FUNCTION anon.fake_siren()");
        return this;
    }

    /// <summary>
    /// Masks the column with a random integer.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    [Obsolete("Use MaskedWithRandomIntBetween(int min, int max) instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public AnonSecurityLabelBuilder MaskedWithRandomInt()
    {
        RawLabel("MASKED WITH FUNCTION anon.random_int()");
        return this;
    }

    /// <summary>
    /// Masks the column with a random integer within the specified range.
    /// </summary>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (inclusive).</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    [Obsolete("Use MaskedWithRandomIntBetween(int min, int max) instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public AnonSecurityLabelBuilder MaskedWithRandomInt(int min, int max)
    {
        if (min > max)
        {
            throw new ArgumentException("Min value cannot be greater than max value.", nameof(min));
        }
        RawLabel($"MASKED WITH FUNCTION anon.random_int_between({min}, {max})");
        return this;
    }

    /// <summary>
    /// Masks the column with partial scrambling, preserving a prefix and suffix.
    /// </summary>
    /// <param name="prefix">The number of characters to preserve at the beginning.</param>
    /// <param name="padding">The padding character to use for the scrambled portion.</param>
    /// <param name="suffix">The number of characters to preserve at the end.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="prefix"/> or <paramref name="suffix"/> is negative.</exception>
    [Obsolete("Use MaskedWithPartialScrambling(string value, int prefixLength, char paddingCharacter, int suffixLength) instead.")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public AnonSecurityLabelBuilder MaskedWithPartialScrambling(int prefix, char padding, int suffix)
    {
        if (prefix < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(prefix), "Prefix cannot be negative.");
        }
        if (suffix < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(suffix), "Suffix cannot be negative.");
        }
        RawLabel($"MASKED WITH FUNCTION anon.partial({prefix}, '{padding}', {suffix})");
        return this;
    }
}
