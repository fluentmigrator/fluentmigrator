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
/// NOISE masking functions for PostgreSQL Anonymizer.
/// These functions add random noise to numeric values while preserving their general scale.
/// </summary>
public partial class AnonSecurityLabelBuilder
{
    /// <summary>
    /// Masks an integer column by adding random noise based on the specified ratio.
    /// </summary>
    /// <param name="noiseRatio">The noise ratio as a decimal (e.g., 0.5 for 50% noise). Must be positive.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="noiseRatio"/> is negative or zero.</exception>
    public AnonSecurityLabelBuilder MaskedWithAddNoiseToInt(double noiseRatio)
    {
        if (noiseRatio <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(noiseRatio), "Noise ratio must be positive.");
        }

        return MaskedWithFunction("anon.add_noise_to_int", noiseRatio);
    }

    /// <summary>
    /// Masks a numeric column by adding random noise based on the specified ratio.
    /// </summary>
    /// <param name="noiseRatio">The noise ratio as a decimal (e.g., 0.5 for 50% noise). Must be positive.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="noiseRatio"/> is negative or zero.</exception>
    public AnonSecurityLabelBuilder MaskedWithAddNoiseToNumeric(double noiseRatio)
    {
        if (noiseRatio <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(noiseRatio), "Noise ratio must be positive.");
        }

        return MaskedWithFunction("anon.add_noise_to_numeric", noiseRatio);
    }
}

