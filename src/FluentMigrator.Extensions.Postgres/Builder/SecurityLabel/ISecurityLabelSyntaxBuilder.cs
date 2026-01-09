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

namespace FluentMigrator.Builder.SecurityLabel;

/// <summary>
/// Provides a fluent interface for building security labels.
/// </summary>
/// <remarks>
/// This builder allows strongly-typed construction of PostgreSQL security labels
/// </remarks>
public interface ISecurityLabelSyntaxBuilder
{
    /// <summary>
    /// Gets the name of the security label provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Specifies a raw security label string.
    /// </summary>
    public void RawLabel(string label);

    /// <summary>
    /// Gets the built label string.
    /// </summary>
    /// <returns>The security label string.</returns>
    string Build();
}
