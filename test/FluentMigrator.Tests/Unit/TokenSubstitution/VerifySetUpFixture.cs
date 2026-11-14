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

using DiffEngine;

using NUnit.Framework;

namespace FluentMigrator.Tests.Unit.TokenSubstitution
{
    /// <summary>
    /// Verify configuration for the token substitution snapshots.
    /// </summary>
    /// <remarks>
    /// Uses a <see cref="SetUpFixtureAttribute"/> rather than a <c>[ModuleInitializer]</c> so the
    /// hook works identically on <c>net48</c> and <c>net8.0</c> without a polyfill.
    /// </remarks>
    [SetUpFixture]
    public class VerifySetUpFixture
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            // Never try to pop a diff tool: this fixture runs unattended on CI and, when the
            // snapshots are intentionally stale (see the "failing matrix" commit), locally too.
            DiffRunner.Disabled = true;
        }
    }
}
