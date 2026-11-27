#region License
//
// Copyright (c) 2026, Fluent Migrator Project
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

namespace FluentMigrator.Tests.SyntaxRoots
{
    /// <summary>
    /// A migration whose <see cref="Up"/> is a lambda, so a test can write a migration with any syntax root.
    /// </summary>
    /// <remarks>
    /// It has no <see cref="MigrationAttribute"/>, so assembly scans skip it, and <c>IMigrationRunner.Up</c> runs it
    /// without reading or writing the version table.
    /// </remarks>
    public sealed class DelegateMigration : Migration
    {
        private readonly Action<Migration> _up;

        public DelegateMigration(Action<Migration> up)
        {
            _up = up;
        }

        public override void Up() => _up(this);

        public override void Down() => throw new NotSupportedException("Only Up is used.");
    }
}
