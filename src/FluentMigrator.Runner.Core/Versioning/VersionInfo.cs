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

using System.Collections.Generic;
using System.Linq;

namespace FluentMigrator.Runner.Versioning
{
    /// <summary>
    /// Represents the versioning information for migrations, providing functionality
    /// to query and update the applied migration versions.
    /// </summary>
    /// <remarks>
    /// This class implements the <see cref="IVersionInfo"/> interface, allowing
    /// management of migration versions, including retrieving the latest version,
    /// adding applied migrations, and checking for specific applied migrations.
    /// </remarks>
    public class VersionInfo : IVersionInfo
    {
        private IList<long> _versionsApplied = new List<long>();

        /// <inheritdoc />
        public long Latest()
        {
            return _versionsApplied.OrderByDescending(x => x).FirstOrDefault();
        }

        /// <inheritdoc />
        public void AddAppliedMigration(long migration)
        {
            _versionsApplied.Add(migration);
        }

        /// <inheritdoc />
        public bool HasAppliedMigration(long migration)
        {
            return _versionsApplied.Contains(migration);
        }

        /// <inheritdoc />
        public IEnumerable<long> AppliedMigrations()
        {
            return _versionsApplied.OrderByDescending(x => x).AsEnumerable();
        }
    }
}
