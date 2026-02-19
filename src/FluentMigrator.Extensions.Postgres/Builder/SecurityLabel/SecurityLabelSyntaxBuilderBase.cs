#region License
// Copyright (c) 2026, Fluent Migrator Project
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
using System.Text;

namespace FluentMigrator.Builder.SecurityLabel;

/// <summary>
/// Provides a base implementation for building security label strings for a specific database provider.
/// </summary>
/// <remarks>
/// Subclasses are expected to use the protected <see cref="Content" /> builder to compose the security label
/// and to expose a provider-specific <see cref="ProviderName" />.
/// </remarks>
/// <example>
/// <code>
/// public sealed class MySepgsqlLabelBuilder : SecurityLabelSyntaxBuilderBase
/// {
///     public override string ProviderName =&gt; "sepgsql";
///
///     public MySepgsqlLabelBuilder SystemObject()
///     {
///         RawLabel("system_u:object_r:sepgsql_table_t:s0");
///         return this;
///     }
/// }
/// </code>
/// </example>
public abstract class SecurityLabelSyntaxBuilderBase : ISecurityLabelSyntaxBuilder
{
    /// <summary>
    /// Gets the string builder used to compose the security label content.
    /// </summary>
    protected StringBuilder Content { get; } = new();

    /// <inheritdoc />
    public abstract string ProviderName { get; }

    /// <inheritdoc />
    public virtual void RawLabel(string label)
    {
        Content.Clear();
        Content.Append(label);
    }

    /// <inheritdoc />
    public virtual string Build()
    {
        if (Content.Length == 0)
        {
            throw new InvalidOperationException("No security label has been specified.");
        }

        return Content.ToString();
    }
}
