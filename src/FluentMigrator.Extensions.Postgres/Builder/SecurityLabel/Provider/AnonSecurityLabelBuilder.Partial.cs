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

namespace FluentMigrator.Builder.SecurityLabel.Provider;

/// <summary>
/// PARTIAL masking functions for PostgreSQL Anonymizer.
/// These functions mask parts of data while preserving prefix and suffix.
/// </summary>
public partial class AnonSecurityLabelBuilder
{
    /// <summary>
    /// Masks the column with partial scrambling, preserving a prefix and suffix.
    /// </summary>
    /// <param name="prefixLength">The number of characters to preserve at the beginning.</param>
    /// <param name="paddingCharacter">The padding character to use for the scrambled portion.</param>
    /// <param name="suffixLength">The number of characters to preserve at the end.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="prefixLength"/> or <paramref name="suffixLength"/> is negative.</exception>
    public AnonSecurityLabelBuilder MaskedWithPartialScrambling(int prefixLength, char paddingCharacter, int suffixLength)
    {
        if (prefixLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(prefixLength), "Prefix length cannot be negative.");
        }

        if (suffixLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(suffixLength), "Suffix length cannot be negative.");
        }

        return MaskedWithFunction("anon.partial", prefixLength, BuildSqlValue(paddingCharacter), suffixLength);
    }
}

