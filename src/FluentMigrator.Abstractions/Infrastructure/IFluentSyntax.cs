#region License
//
// Copyright (c) 2007-2018, Sean Chambers <schambers80@gmail.com>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.ComponentModel;

namespace FluentMigrator.Infrastructure
{
    /// <summary>
    /// The base interface for the fluent API
    /// </summary>
    /// <remarks>
    /// This is just here to suppress the default members of <see cref="object"/>
    /// </remarks>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IFluentSyntax
    {
        /// <summary>
        /// Gets the objects type
        /// </summary>
        /// <returns>The objects type</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Type GetType();

        /// <summary>
        /// Gets the objects hash code
        /// </summary>
        /// <returns>The objects hash code</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();

        /// <summary>
        /// Gets the objects string representation
        /// </summary>
        /// <returns>The objects string representation</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        string ToString();

        /// <summary>
        /// Compares if two objects of the same type are equal
        /// </summary>
        /// <param name="other">The object this one should be compared to</param>
        /// <returns><c>true</c> when both objects are equal</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object other);
    }
}
