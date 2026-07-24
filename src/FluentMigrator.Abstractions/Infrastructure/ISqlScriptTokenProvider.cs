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

using System.Collections.Generic;

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// Provides a map of well-known token variables (e.g. <c>DefaultSchema</c>) that can be
    /// referenced from <c>Execute.Sql</c>, <c>Execute.Script</c>, and <c>Execute.EmbeddedScript</c>
    /// SQL text using the <c>$(name)</c>/<c>$[name]</c> token syntax supported by
    /// <see cref="SqlScriptTokenReplacer"/>.
    /// </summary>
    /// <remarks>
    /// Multiple implementations can be registered with the dependency injection container.
    /// All registered token maps are merged (in registration order), and any user-supplied
    /// <c>Parameters</c> dictionary passed to <c>Execute.Sql</c>/<c>Execute.Script</c>/
    /// <c>Execute.EmbeddedScript</c> takes precedence over well-known tokens with the same name.
    /// </remarks>
    public interface ISqlScriptTokenProvider
    {
        /// <summary>
        /// Gets the map of token names to their values
        /// </summary>
        /// <returns>The token map</returns>
        IDictionary<string, string> GetTokens();
    }
}
