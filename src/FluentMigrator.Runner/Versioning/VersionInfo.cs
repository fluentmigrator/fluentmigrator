#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
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
    public class VersionInfo : FluentMigrator.Runner.Versioning.IVersionInfo
    {
        private IList<long> _versionsApplied = new List<long>();

        public long Latest()
        {
            return _versionsApplied.OrderByDescending(x => x).FirstOrDefault();
        }

        public void AddAppliedMigration(long migration)
        {
            _versionsApplied.Add(migration);
        }

        public bool HasAppliedMigration(long migration)
        {
            return _versionsApplied.Contains(migration);
        }

        public IEnumerable<long> AppliedMigrations()
        {
            return _versionsApplied.OrderByDescending(x => x).AsEnumerable();
        }
    }
}
