using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentMigrator.Runner.Versioning
{
    public class VersionInfo
    {
        public const string TABLE_NAME = "VersionInfo";

        public long CurrentVersion { get; private set; }
        public long PreviousVersion { get; private set; }
        public DateTime LastUpdated { get; private set; }

        public VersionInfo(long currentVersion, long previousVersion, DateTime lastUpdated)
        {
            this.CurrentVersion = currentVersion;
            this.PreviousVersion = previousVersion;
            this.LastUpdated = lastUpdated;
        }
    }
}
