#region License
//
// Copyright (c) 2018, Fluent Migrator Project
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

using JetBrains.Annotations;

namespace FluentMigrator.Runner.Processors
{
    [Obsolete]
    public interface IMigrationProcessorFactory
    {
        [Obsolete]
        [NotNull]
        IMigrationProcessor Create(string connectionString, [NotNull] IAnnouncer announcer, [NotNull] IMigrationProcessorOptions options);

        /// <summary>
        /// Returns a value indicating whether this processor factory can use the given DB provider
        /// </summary>
        /// <param name="provider">The DB provider name</param>
        /// <returns><c>true</c> when this processor factory can use the given DB provider</returns>
        [Obsolete]
        bool IsForProvider([NotNull] string provider);

        [NotNull]
        string Name { get; }
    }
}
