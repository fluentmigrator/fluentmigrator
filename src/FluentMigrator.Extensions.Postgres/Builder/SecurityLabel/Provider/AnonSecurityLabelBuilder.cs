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
/// Implementation of the PostgreSQL Anonymizer security label builder.
/// Provides strongly-typed methods for creating masking rules using PostgreSQL Anonymizer extension.
/// This is the core partial class containing the base infrastructure and essential methods.
/// </summary>
public partial class AnonSecurityLabelBuilder : SecurityLabelSyntaxBuilderBase
{
    /// <inheritdoc/>
    public override string ProviderName => "anon";

    /// <summary>
    /// Builds an array of SQL-formatted argument strings from the provided values.
    /// </summary>
    protected static string[] BuildArgsArray(params object[] args)
    {
        if (args is null || args.Length == 0)
        {
            return [];
        }

        var argsArray = new string[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            argsArray[i] = BuildSqlValue(args[i]);

        }
        return argsArray;
    }

    /// <summary>
    /// Builds the SQL representation of a value for use in function calls.
    /// </summary>
    protected static string BuildSqlValue(object value) => value switch
    {
        null => "NULL",
        RawSql sql => sql.Value,
        string str => str, // Strings are handled as-is to allow column names
        char c => $"'{c}'",
        bool b => b ? "TRUE" : "FALSE",
        DateTime dt => BuildSqlString(dt.ToString("yyyy-MM-dd HH:mm:ss")),
        DateTimeOffset dto => BuildSqlString(dto.ToString("yyyy-MM-dd HH:mm:ss zzz")),
        int or long or short or byte or uint or ulong or ushort or sbyte or float or double or decimal => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture),
        _ => BuildSqlValue(value.ToString()),
    };

    /// <summary>
    /// Escapes and formats a string for SQL usage.
    /// </summary>
    protected static string BuildSqlString(string value) => $"'{value.Replace("'", "''")}'";

    /// <summary>
    /// Masks the column with a static value.
    /// </summary>
    /// <param name="value">The static value to use for masking.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
        }

        RawLabel($"MASKED WITH VALUE {BuildSqlString(value)}");
        return this;
    }

    /// <summary>
    /// Masks the column with a custom function call.
    /// </summary>
    /// <param name="functionName">The function name, or call expression (e.g., "anon.my_function()").</param>
    /// <param name="args">Function params, if no parentheses are provided in <paramref name="functionName"/>.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="functionName"/> is null or whitespace.</exception>
    public AnonSecurityLabelBuilder MaskedWithFunction([NotNull] string functionName, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(functionName))
        {
            throw new ArgumentException("Function name cannot be null or whitespace.", nameof(functionName));
        }

        // If parentheses already present, use as-is; otherwise ensure parentheses.
        if (functionName.IndexOf('(') >= 0)
        {
            RawLabel($"MASKED WITH FUNCTION {functionName}");
            return this;
        }

        // No parameters
        if (args is null || args.Length == 0)
        {
            RawLabel($"MASKED WITH FUNCTION {functionName}()");
            return this;
        }

        // Build anon.<functionName>(args...)
        var argsPart = "(" + string.Join(", ", BuildArgsArray(args)) + ")";
        RawLabel($"MASKED WITH FUNCTION {functionName}{argsPart}");
        return this;
    }


    /// <summary>
    /// Marks the column as masked without specifying a masking function.
    /// This is typically used for dynamic masking where the function is determined at runtime.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public AnonSecurityLabelBuilder Masked()
    {
        RawLabel("MASKED");
        return this;
    }
}
