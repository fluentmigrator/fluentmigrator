#region License
// Copyright (c) 2018, Fluent Migrator Project
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

namespace FluentMigrator.Runner.BatchParser
{
    /// <summary>
    /// Event arguments for a special token
    /// </summary>
    public class SpecialTokenEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecialTokenEventArgs"/> class.
        /// </summary>
        /// <param name="token">The found token</param>
        /// <param name="opaque">An opaque (token specific) value</param>
        public SpecialTokenEventArgs([NotNull] string token, object opaque)
        {
            Token = token;
            Opaque = opaque;
        }

        /// <summary>
        /// Gets the found token
        /// </summary>
        [NotNull]
        public string Token { get; }

        /// <summary>
        /// Gets an opaque (token specific) value
        /// </summary>
        [CanBeNull]
        public object Opaque { get; }
    }
}
