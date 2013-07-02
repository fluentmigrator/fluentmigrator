using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator.Runner.Versioning
{
    public class ConnectionlessVersionInfo : IVersionInfo
    {
        private readonly long _startingVersion;
        public ConnectionlessVersionInfo(long startingVersion)
        {
            _startingVersion = startingVersion;
        }
        public void AddAppliedMigration(long migration)
        {
            
        }

        public IEnumerable<long> AppliedMigrations()
        {
            return new [] {_startingVersion};
        }

        public bool HasAppliedMigration(long migration)
        {
            return migration < _startingVersion;
        }

        public long Latest()
        {
            return _startingVersion;
        }
    }
}
