#region License
//
// Copyright (c) 2007-2024, Fluent Migrator Project
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

using System.Runtime.CompilerServices;

namespace FluentMigrator
{
    /// <summary>
    /// Provides a centralized check for whether dynamic code (reflection emit, etc.) is supported
    /// at runtime. In production builds this delegates directly to
    /// <c>System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported</c>
    /// (not a <c>cref</c>: the property does not exist on net48/netstandard2.0),
    /// preserving the trimmer's ability to eliminate unreachable code paths.
    /// When <c>TESTING_AOT</c> is defined (via <c>-p:TestingAot=true</c>), the property
    /// returns <c>false</c> so that unit tests can exercise AOT code paths on a normal runtime.
    /// </summary>
    internal static class AotSupport
    {
#if TESTING_AOT
        /// <summary>
        /// Always returns <c>false</c> when <c>TESTING_AOT</c> is defined,
        /// simulating an AOT environment for testing purposes.
        /// </summary>
        internal static bool IsDynamicCodeSupported => false;
#elif NET
        /// <summary>
        /// Returns the runtime value of
        /// <see cref="System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported"/>.
        /// </summary>
        internal static bool IsDynamicCodeSupported
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => RuntimeFeature.IsDynamicCodeSupported;
        }
#else
        /// <summary>
        /// On .NET Framework, dynamic code is always supported.
        /// </summary>
        internal static bool IsDynamicCodeSupported => true;
#endif
    }
}
