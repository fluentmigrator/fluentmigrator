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
/// Hash algorithm constants for PostgreSQL Anonymizer hashing functions.
/// </summary>
public static class AnonHashAlgorithm
{
    /// <summary>
    /// MD5 hash algorithm (default).
    /// </summary>
    public const string Md5 = "md5";

    /// <summary>
    /// SHA1 hash algorithm.
    /// </summary>
    public const string Sha1 = "sha1";

    /// <summary>
    /// SHA256 hash algorithm.
    /// </summary>
    public const string Sha256 = "sha256";

    /// <summary>
    /// SHA384 hash algorithm.
    /// </summary>
    public const string Sha384 = "sha384";

    /// <summary>
    /// SHA512 hash algorithm.
    /// </summary>
    public const string Sha512 = "sha512";
}

/// <summary>
/// HASHING masking functions for PostgreSQL Anonymizer.
/// These functions apply cryptographic hash algorithms to replace data with hash digests.
/// </summary>
public partial class AnonSecurityLabelBuilder
{
    /// <summary>
    /// Masks the column with a hash digest using MD5 (default algorithm).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithHash()
    {
        return MaskedWithFunction("anon.hash");
    }

    /// <summary>
    /// Masks the column with a hash digest using the specified algorithm.
    /// </summary>
    /// <param name="algorithm">The hash algorithm to use (e.g., "md5", "sha256", "sha512"). Use constants from <see cref="AnonHashAlgorithm"/>.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="algorithm"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithHash([NotNull] string algorithm)
    {
        if (string.IsNullOrWhiteSpace(algorithm))
        {
            throw new ArgumentException("Algorithm cannot be null or whitespace.", nameof(algorithm));
        }

        return MaskedWithFunction("anon.hash", algorithm);
    }

    /// <summary>
    /// Masks the column with an HMAC hash using MD5 (default algorithm).
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder MaskedWithHmacHash()
    {
        return MaskedWithFunction("anon.hmac_hash");
    }

    /// <summary>
    /// Masks the column with an HMAC hash using the specified algorithm.
    /// </summary>
    /// <param name="algorithm">The hash algorithm to use (e.g., "md5", "sha256", "sha512"). Use constants from <see cref="AnonHashAlgorithm"/>.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="algorithm"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithHmacHash([NotNull] string algorithm)
    {
        if (string.IsNullOrWhiteSpace(algorithm))
        {
            throw new ArgumentException("Algorithm cannot be null or whitespace.", nameof(algorithm));
        }

        return MaskedWithFunction("anon.hmac_hash", algorithm);
    }
}

